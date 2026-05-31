using System.Collections.Generic;
using UnityEngine;

namespace recordPlayer
{
    public class RecordPlayerController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private AudioSource audioSource;

        [Header("Record")]
        [SerializeField] private RecordData currentRecord;

        [Header("Animation")]
        [SerializeField] private Transform toneArmPivot;
        [SerializeField] private Transform platter;
        [SerializeField] private Transform vinyl;        
        [SerializeField] private float armRestAngle = 0f;
        [SerializeField] private float armPlayAngle = 45f;
        [SerializeField] private float armMoveSpeed = 1.5f;

        private bool _isPlaying;
        private int _currentTrackIndex;

        private float _currentRPM = 33f;

        private void Awake()
        {
            _isPlaying = false;
            _currentTrackIndex = 0;
        }

        void Update()
        {

            
            if (_isPlaying)
            {
                // Plattenteller dreht sich
                platter.Rotate(Vector3.forward, _currentRPM * 6f * Time.deltaTime);
                // Vinyl dreht sich
                vinyl.Rotate(Vector3.up, _currentRPM * 6f * Time.deltaTime);
            }

            // Tonarm bewegt sich (Z-Achse)
            float targetAngle = _isPlaying ? armPlayAngle : armRestAngle;
            float currentAngle = toneArmPivot.localEulerAngles.z;
            float newAngle = Mathf.LerpAngle(currentAngle, targetAngle, armMoveSpeed * Time.deltaTime);
            toneArmPivot.localEulerAngles = new Vector3(
                toneArmPivot.localEulerAngles.x,
                toneArmPivot.localEulerAngles.y,
                newAngle
            );
        }

        public void TogglePlay()
        {
            if (_isPlaying) Stop();
            else Play();
        }

        public void Play()
        {
            if (currentRecord == null || currentRecord.TrackCount == 0) return;

            audioSource.clip = currentRecord.GetTrack(_currentTrackIndex);
            audioSource.pitch = currentRecord.speed switch
            {
                Speed.Slow => 0.65f,
                Speed.Fast => 1.25f,
                Speed.Normal => 1f,
                _ => 1f
            };

            audioSource.Play();
            _isPlaying = true;
        }

        public void Stop()
        {
            audioSource.Stop();
            _isPlaying = false;
        }

        public void NextTrack()
        {
            if (currentRecord == null) return;
            _currentTrackIndex = (_currentTrackIndex + 1) % currentRecord.TrackCount;
            if (_isPlaying) Play();
        }

        public void PreviousTrack()
        {
            if (currentRecord == null) return;
            _currentTrackIndex = (_currentTrackIndex - 1 + currentRecord.TrackCount) % currentRecord.TrackCount;
            if (_isPlaying) Play();
        }

        public void SetRecord(RecordData record)
        {
            Stop();
            currentRecord = record;
            _currentTrackIndex = 0;
        }

        public void SetPitch(float rpm)
        {
            _currentRPM = rpm;
            audioSource.pitch = rpm == 45f ? 1.25f : 0.65f;
        }
    }
}