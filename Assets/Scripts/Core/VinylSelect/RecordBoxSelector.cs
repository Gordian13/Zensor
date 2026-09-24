using System.Collections.Generic;
using Core.camera;
using Core.VinylSelect;
using Interaction.util.ColorReveal;
using UnityEngine;
using UnityEngine.InputSystem;

namespace record
{
    /**
     * @brief Handles record hover and selection at the active vinyl browsing spot.
     *
     * Assign vinylSelectController, targetCamera, spotManager and owningSpot in the
     * Inspector. Missing camera/spot references are searched for in Awake; if either
     * spotManager or owningSpot remains missing, the spot guard permits interaction.
     * recordInfoUI is optional and can display metadata while hovering.
     *
     * recordLayer identifies selectable records. blockingLayer must contain both
     * record colliders and walls or other occluders: only the nearest non-trigger hit
     * is considered, and a non-record hit stops selection. An empty blockingLayer
     * falls back to Physics.DefaultRaycastLayers. rayDistance limits the query.
     * Hover/selection runs only in BrowsingBox and pauses during a global interaction
     * block. Leaving browsing or the owning spot restores the current hover position.
     * @see VinylSelectController
     */
    public class RecordBoxSelector : MonoBehaviour
    {
        [Header("References")] [SerializeField]
        private Camera targetCamera;

        [SerializeField] private RecordInfoUI recordInfoUI;

        [Header("Raycast")] [SerializeField] private LayerMask recordLayer = ~0;
        [SerializeField] private LayerMask blockingLayer = Physics.DefaultRaycastLayers;
        [SerializeField] private float rayDistance = 100f;

        [Header("Hover Animation")] [SerializeField]
        private Vector3 hoverOffset = new Vector3(0f, 0.15f, 0f);

        [SerializeField] private float moveSpeed = 8f;
        [SerializeField] private bool hideUIWhenNotHovering = true;

        [Header("State Controller")] [SerializeField]
        private VinylSelectController vinylSelectController;

        [Header("Spot Guard")] [SerializeField]
        private SpotManager spotManager;

        [SerializeField] private CameraSpot owningSpot;

        private readonly Dictionary<Transform, Vector3> restPositions = new();
        private Transform hoveredTransform;
        private bool wasOwningSpotActive = false;

        private void Awake()
        {
            if (targetCamera == null)
                targetCamera = GetComponent<Camera>();

            if (targetCamera == null)
                targetCamera = Camera.main;

            if (targetCamera == null)
                Debug.LogError($"{nameof(RecordBoxSelector)} could not find a camera.", this);

            if (vinylSelectController == null)
                Debug.LogError($"{nameof(RecordBoxSelector)} has no VinylSelectController assigned.", this);

            if (spotManager == null)
                spotManager = FindFirstObjectByType<SpotManager>();

            if (owningSpot == null)
                owningSpot = GetComponentInParent<CameraSpot>();
        }

        private void Update()
        {
            if (GlobalInteractionState.Instance != null &&
                GlobalInteractionState.Instance.IsInteractionBlocked)
                return;

            bool isOwningSpotActive = IsOwningSpotActive() &&
                                      vinylSelectController != null &&
                                      vinylSelectController.CurrentVinylState == VinylState.BrowsingBox;

            if (!isOwningSpotActive)
            {
                ClearHover(true);
                wasOwningSpotActive = false;
                return;
            }

            // First frame back on this spot: snap all records to their rest positions
            // so colliders and visuals are in sync before raycasting begins.
            if (!wasOwningSpotActive)
            {
                SnapAllRecordsToRest();
                wasOwningSpotActive = true;
            }

            IVinyl hoveredVinyl = GetVinylUnderCursor(out Transform vinylTransform);

            if (vinylTransform != hoveredTransform)
                ChangeHoveredRecord(hoveredVinyl, vinylTransform);

            Mouse mouse = Mouse.current;
            if (hoveredVinyl != null &&
                mouse != null &&
                mouse.leftButton.wasPressedThisFrame &&
                vinylSelectController.SelectVinyl(hoveredVinyl))
            {
                ClearHover(true);
                return;
            }

            AnimateRecords();
        }

        private void ChangeHoveredRecord(IVinyl vinyl, Transform vinylTransform)
        {
            hoveredTransform = vinylTransform;

            if (hoveredTransform != null)
            {
                if (!restPositions.ContainsKey(hoveredTransform))
                    restPositions.Add(hoveredTransform, hoveredTransform.localPosition);

                recordInfoUI?.ShowData(vinyl.GetData());
            }
            else if (hideUIWhenNotHovering)
            {
                recordInfoUI?.Hide();
            }
        }

        private void AnimateRecords()
        {
            foreach (KeyValuePair<Transform, Vector3> record in restPositions)
            {
                if (record.Key == null)
                    continue;

                Vector3 targetPosition = record.Value;
                if (record.Key == hoveredTransform)
                {
                    targetPosition += record.Key.localRotation * Vector3.forward * hoverOffset.y;
                }

                record.Key.localPosition = Vector3.Lerp(
                    record.Key.localPosition,
                    targetPosition,
                    moveSpeed * Time.deltaTime
                );

                if (Vector3.SqrMagnitude(record.Key.localPosition - targetPosition) < 0.000001f)
                    record.Key.localPosition = targetPosition;
            }
        }

        /**
         * @brief Resolves a record from the nearest blocking collider under the mouse.
         * @param[out] vinylTransform Selection transform of the accepted record, or null.
         * @return An IVinyl provider with vinyl-format data, or null when no valid record
         * is hit. Objects behind the first blocking hit are never considered.
         */
        private IVinyl GetVinylUnderCursor(out Transform vinylTransform)
        {
            vinylTransform = null;

            Mouse mouse = Mouse.current;
            if (targetCamera == null || mouse == null)
                return null;
            
            Physics.SyncTransforms();

            Ray ray = targetCamera.ScreenPointToRay(mouse.position.ReadValue());
            if (!Physics.Raycast(
                ray,
                out RaycastHit hit,
                rayDistance,
                GetBlockingLayerMask(),
                QueryTriggerInteraction.Ignore))
            {
                return null;
            }

            if (!IsLayerInMask(hit.collider.gameObject.layer, recordLayer))
                return null;

            return GetVinylFromHit(hit, out vinylTransform);
        }

        /**
         * @brief Clears the current hover and optionally restores its position immediately.
         * @param restoreImmediately Whether to snap the hovered record to its cached position.
         */
        private void ClearHover(bool restoreImmediately = false)
        {
            if (hoveredTransform == null)
                return;

            if (restoreImmediately &&
                restPositions.TryGetValue(hoveredTransform, out Vector3 restPosition))
            {
                hoveredTransform.localPosition = restPosition;
            }

            hoveredTransform = null;

            if (hideUIWhenNotHovering)
                recordInfoUI?.Hide();
        }

        // Immediately snaps every tracked record to its stored rest position.
        // Called on the first frame after returning to the vinyl spot so that
        // colliders and visuals are in sync before raycasting starts.
        private void SnapAllRecordsToRest()
        {
            foreach (KeyValuePair<Transform, Vector3> record in restPositions)
            {
                if (record.Key != null)
                    record.Key.localPosition = record.Value;
            }
        }

        /**
         * @brief Checks whether this selector belongs to the current camera spot.
         * @return True for the current spot, or when either guard reference is missing.
         */
        private bool IsOwningSpotActive()
        {
            if (spotManager == null || owningSpot == null)
                return true;

            return spotManager.IsCurrentSpot(owningSpot);
        }

        private int GetBlockingLayerMask()
        {
            return blockingLayer.value != 0
                ? blockingLayer.value
                : Physics.DefaultRaycastLayers;
        }

        private static bool IsLayerInMask(int layer, LayerMask layerMask)
        {
            return (layerMask.value & (1 << layer)) != 0;
        }

        private static IVinyl GetVinylFromHit(RaycastHit hit, out Transform vinylTransform)
        {
            vinylTransform = null;

            MonoBehaviour[] components =
                hit.collider.GetComponentsInParent<MonoBehaviour>(true);

            foreach (MonoBehaviour component in components)
            {
                if (component is not IVinyl vinyl)
                    continue;

                RecordData data = vinyl.GetData();
                if (data == null || data.format != RecordFormat.Vinyl)
                    continue;

                vinylTransform = vinyl.GetSelectionTransform();
                return vinyl;
            }

            return null;
        }
    }
}
