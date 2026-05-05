using System;
using NUnit.Framework.Constraints;
using UnityEngine;

namespace record
{
    public class RecordInteractionSkript : MonoBehaviour, IVinyl
    {
        public RecordData data;
        public int rotationSpeed = 10;
        private bool isPlaying;
        
        public RecordData GetData() => data;

        public void Start()
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

        public void Update()
        {
            if (!isPlaying) return;
            
            transform.Rotate(Vector3.up * (rotationSpeed * Time.deltaTime));
            Debug.Log("Record update");
        }
    }
}
