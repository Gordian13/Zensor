using UnityEngine;

namespace record
{
    public class RecordInteractionScript : MonoBehaviour, IVinyl
    {
        [SerializeField] private RecordData data;

        [Header("Vinyl Parts")]
        [SerializeField] private Transform vinylDisc;

        public RecordData GetData() => data;
        public Transform GetSelectionTransform() => transform;
        public Transform GetVinylDiscTransform() => vinylDisc;
    }
}
