using UnityEngine;

namespace record
{
    public class RecordInteractionScript : MonoBehaviour, IVinyl
    {
        [SerializeField] private RecordData data;

        [Header("Vinyl Parts")]
        [SerializeField] private Transform cover;
        [SerializeField] private Transform vinylDisc;
        
        [Header("Playback")]
        [SerializeField] private int rotationSpeed = 10;

        private bool isPlaying;
        
        public RecordData GetData() => data;
        public Transform GetSelectionTransform() => transform;
        public Transform GetCoverTransform() => cover;
        public Transform GetVinylDiscTransform() => vinylDisc;

        private void Awake()
        {
            isPlaying = false;
        }

        public void OnPlaced()
        {
            isPlaying = true;
            Debug.Log("Record placed");
        }

        public void OnRemoved()
        {
            isPlaying = false;
            Debug.Log("Record removed");
        }

        private void Update()
        {
            if (!isPlaying) return;
            
            transform.Rotate(Vector3.up * (rotationSpeed * Time.deltaTime));
        }
    }
}
