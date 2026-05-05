using record;
using recordPlayer;
using UnityEngine;

namespace GameScripts
{
    public class DragRigidbody : MonoBehaviour, IDraggable
    {
        private Rigidbody _rb;
        private Vector3 _offset;
        private bool _wasKinematic;
        private bool _hadGravity;

        void Start()
        {
            _rb = GetComponent<Rigidbody>();
            if (_rb != null)
            {
                _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            }
            else
            {
                Debug.LogError($"No Rigidbody found on {gameObject.name}.");
            }
        }

        public void OnBeginDrag(Vector3 mouseWorldPos)
        {
            // Notify record player that dragging starts (release if currently playing)
            if (TryGetComponent<RecordInteractionSkript>(out _))
            {
                var recordPlayer = FindFirstObjectByType<RecordPlayerController>();
                if (recordPlayer != null)
                {
                    recordPlayer.ReleaseCurrentRecord();
                }
            }

            if (_rb == null) return;

            // Store initial state for potential rollback
            _wasKinematic = _rb.isKinematic;
            _hadGravity = _rb.useGravity;

            // Make kinematic for smooth dragging
            _rb.isKinematic = true;
            _offset = transform.position - mouseWorldPos;
        }

        public void OnDrag(Vector3 mouseWorldPos)
        {
            if (_rb == null) return;
            _rb.MovePosition(mouseWorldPos + _offset);
        }

        public void OnEndDrag()
        {
            if (_rb == null) return;

            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;

            // Try to snap to record player if this is a record
            bool snappedSuccessfully = false;
            if (TryGetComponent<RecordInteractionSkript>(out var vinyl))
            {
                var recordPlayer = FindFirstObjectByType<RecordPlayerController>();
                if (recordPlayer != null)
                {
                    snappedSuccessfully = recordPlayer.TrySnapRecord(vinyl, _rb);
                }
            }

            // If snap failed, restore normal physics
            if (!snappedSuccessfully)
            {
                _rb.isKinematic = _wasKinematic;
                _rb.useGravity = _hadGravity;
            }
        }
    }
}
