using record;
using UnityEngine;

namespace recordPlayer
{
    public class RecordPlayerController : MonoBehaviour
    {
        [SerializeField] private Transform snapPoint;
        [SerializeField] private float snapRadius = 0.5f;

        private IVinyl _recordToPlay;
        private Rigidbody _rbRecordToPlay;
        private AudioSource _audioSource;
        private bool _isPlaying;
        private float _checkTimer;
        private const float CheckInterval = 3f;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            _isPlaying = false;
            _checkTimer = CheckInterval;
        }

        private void Update()
        {
            _checkTimer += Time.deltaTime;
        }

        private void OnTriggerStay(Collider other)
        {
            if (_isPlaying || _recordToPlay != null) return;
            if (_checkTimer < CheckInterval) return;

            _checkTimer = 0f;

            if (other.TryGetComponent<RecordInteractionSkript>(out var vinyl))
            {
                var rb = other.attachedRigidbody != null ? other.attachedRigidbody : other.GetComponent<Rigidbody>();
                TrySnapRecord(vinyl, rb);
            }
        }
        
        private void OnTriggerExit(Collider other)
        {
            if (!_isPlaying || _recordToPlay == null) return;

            if (other.TryGetComponent<RecordInteractionSkript>(out var vinyl) && ReferenceEquals(vinyl, _recordToPlay))
            {
                ReleaseCurrentRecord();
            }
        }

        public bool TrySnapRecord(RecordInteractionSkript vinyl, Rigidbody vinylRb)
        {
            if (vinyl == null) return false;

            if (_isPlaying && ReferenceEquals(_recordToPlay, vinyl))
            {
                return true;
            }

            if (_isPlaying || _recordToPlay != null)
            {
                return false;
            }

            var snapPos = GetSnapPointPosition();
            var sqrDistance = (vinyl.transform.position - snapPos).sqrMagnitude;
            if (sqrDistance > snapRadius * snapRadius)
            {
                return false;
            }

            _isPlaying = true;
            _recordToPlay = vinyl;
            _rbRecordToPlay = vinylRb != null ? vinylRb : vinyl.GetComponent<Rigidbody>();

            if (_rbRecordToPlay != null)
            {
                _rbRecordToPlay.linearVelocity = Vector3.zero;
                _rbRecordToPlay.angularVelocity = Vector3.zero;
                _rbRecordToPlay.useGravity = false;
                _rbRecordToPlay.isKinematic = true;
            }

            var target = snapPoint != null ? snapPoint : transform;
            vinyl.transform.position = target.position;
            vinyl.transform.rotation = target.rotation;

            _recordToPlay.OnPlaced();

            var clip = vinyl.GetData().audioClip;
            if (clip != null)
            {
                _audioSource.clip = clip;
                _audioSource.Play();
            }

            return true;
        }

        public void ReleaseCurrentRecord()
        {
            if (_recordToPlay == null) return;

            _isPlaying = false;
            _checkTimer = 0f;

            if (_rbRecordToPlay != null)
            {
                _rbRecordToPlay.useGravity = true;
                _rbRecordToPlay.isKinematic = false;
            }

            _recordToPlay.OnRemoved();
            _audioSource.Stop();
            _rbRecordToPlay = null;
            _recordToPlay = null;
        }

        public Vector3 GetSnapPointPosition()
        {
            return snapPoint != null ? snapPoint.position : transform.position;
        }
    }
}