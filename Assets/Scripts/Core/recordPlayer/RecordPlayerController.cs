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
        [SerializeField] private Vector3 vinylRotationAxis = Vector3.up;
        [SerializeField] private float armRestAngle = 0f;
        [SerializeField] private float armPlayAngle = 45f;
        [SerializeField] private float armEndAngle = 80f;
        [SerializeField] private float armMoveSpeed = 1.5f;

        [Header("Cover Display")]
        [SerializeField] private GameObject coverObject;
        [SerializeField] private Renderer coverRenderer;

        public int CurrentTrackIndex => _currentTrackIndex;
        public int TrackCount => currentRecord?.TrackCount ?? 0;
        public string CurrentTrackName => currentRecord?.GetTrack(_currentTrackIndex)?.name ?? "";

        private bool _isPlaying;
        private int _currentTrackIndex;
        private float _currentRPM = 33f;
        private Renderer _vinylRenderer;
        private Renderer[] _vinylRenderers;
        private float _totalDuration;
        private float _playedBeforeCurrentTrack;

        private void Awake()
        {
            _isPlaying = false;
            _currentTrackIndex = 0;

            if (coverObject != null)
                coverObject.SetActive(false);

            if (vinyl != null)
            {
                _vinylRenderers = vinyl.GetComponentsInChildren<Renderer>(true);
                foreach (Renderer r in _vinylRenderers)
                {
                    if (r.name == "LabelFront")
                        _vinylRenderer = r;
                }
                SetVinylVisible(false);
            }
        }

        void Update()
        {
            if (_isPlaying)
            {
                platter.Rotate(Vector3.forward, _currentRPM * 6f * Time.deltaTime);
                vinyl.Rotate(vinylRotationAxis, _currentRPM * 6f * Time.deltaTime);
            }

            float targetAngle;
            if (_isPlaying && _totalDuration > 0)
                targetAngle = Mathf.Lerp(armPlayAngle, armEndAngle, (_playedBeforeCurrentTrack + audioSource.time) / _totalDuration);
            else if (currentRecord != null && _totalDuration > 0)
                targetAngle = Mathf.Lerp(armPlayAngle, armEndAngle, _playedBeforeCurrentTrack / _totalDuration);
            else
                targetAngle = armRestAngle;

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
            if (_currentTrackIndex >= currentRecord.TrackCount - 1) return;

            AudioClip current = currentRecord.GetTrack(_currentTrackIndex);
            if (current != null) _playedBeforeCurrentTrack += current.length;
            _currentTrackIndex++;
            SnapArmToProgress();
            if (_isPlaying) Play();
        }

        public void PreviousTrack()
        {
            if (currentRecord == null) return;
            if (_currentTrackIndex <= 0) return;

            _currentTrackIndex--;
            AudioClip prev = currentRecord.GetTrack(_currentTrackIndex);
            if (prev != null) _playedBeforeCurrentTrack -= prev.length;
            _playedBeforeCurrentTrack = Mathf.Max(0f, _playedBeforeCurrentTrack);
            SnapArmToProgress();
            if (_isPlaying) Play();
        }

        private void SnapArmToProgress()
        {
            if (_totalDuration <= 0) return;
            float progress = _playedBeforeCurrentTrack / _totalDuration;
            float angle = Mathf.Lerp(armPlayAngle, armEndAngle, progress);
            toneArmPivot.localEulerAngles = new Vector3(
                toneArmPivot.localEulerAngles.x,
                toneArmPivot.localEulerAngles.y,
                angle
            );
        }

        public void SetRecord(RecordData record)
        {
            Stop();
            currentRecord = record;
            _currentTrackIndex = 0;
            _playedBeforeCurrentTrack = 0f;
            _totalDuration = 0f;
            for (int i = 0; i < record.TrackCount; i++)
            {
                AudioClip clip = record.GetTrack(i);
                if (clip != null) _totalDuration += clip.length;
            }
            ApplyLabel(record);
            ApplyCover(record);
            SetVinylVisible(true);
        }

        public void ClearRecord()
        {
            Stop();
            currentRecord = null;
            _currentTrackIndex = 0;
            ApplyCover(null);
            SetVinylVisible(false);
        }

        private void SetVinylVisible(bool visible)
        {
            if (_vinylRenderers == null) return;
            foreach (Renderer r in _vinylRenderers)
                r.enabled = visible;
        }

        public void SetPitch(float rpm)
        {
            _currentRPM = rpm;
            audioSource.pitch = rpm == 45f ? 1.25f : 0.65f;
        }

        private void ApplyLabel(RecordData record)
        {
            if (_vinylRenderer == null || record == null || record.labelFrontTexture == null) return;

            Material mat = _vinylRenderer.material;
            if (mat.HasProperty("_BaseMap"))
                mat.SetTexture("_BaseMap", record.labelFrontTexture);
            else if (mat.HasProperty("_MainTex"))
                mat.SetTexture("_MainTex", record.labelFrontTexture);
        }

        private void ApplyCover(RecordData record)
        {
            bool hasRecord = record != null && record.coverFrontTexture != null;

            if (coverObject != null)
                coverObject.SetActive(hasRecord);

            if (coverRenderer != null && hasRecord)
            {
                Material mat = coverRenderer.material;
                if (mat.HasProperty("_BaseMap"))
                    mat.SetTexture("_BaseMap", record.coverFrontTexture);
                else if (mat.HasProperty("_MainTex"))
                    mat.SetTexture("_MainTex", record.coverFrontTexture);
            }
        }
    }
}
