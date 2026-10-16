using UnityEngine;

namespace record
{
    /**
     * Component on each vinyl record in the scene.
     *
     * Holds the record's data and references to its parts, and exposes them
     * through IVinyl so that the selection, inspection and player scripts
     * can work with the record.
     */
    public class RecordInteractionScript : MonoBehaviour, IVinyl
    {
        /** Data of this record */
        [Header("Vinyl Data")]
        [SerializeField] private RecordData data;

        /** The disc object inside the sleeve. */
        [Header("Vinyl Parts")]
        [SerializeField] private Transform vinylDisc;
        
        public RecordData GetData() => data;
        public Transform GetSelectionTransform() => transform;
        public Transform GetVinylDiscTransform() => vinylDisc;
    }
}
