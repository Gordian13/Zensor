using UnityEngine;

namespace record
{
    public class RecordInteractionScript : MonoBehaviour, IVinyl
    {
        [SerializeField] private RecordData data;
        [SerializeField] private Transform selectionRoot;
        [SerializeField] private int rotationSpeed = 10;

        private bool isPlaying;
        
        public RecordData GetData() => data;
        public Transform GetSelectionTransform() => selectionRoot != null ? selectionRoot : transform;

        private void Awake()
        {
            if (selectionRoot == null)
                selectionRoot = transform;

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
