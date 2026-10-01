using System.Collections.Generic;
using Core.camera;
using Core.VinylSelect;
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

        /**
         * Searches for the camera, SpotManager and CameraSpot if they are not set in the Inspector.
         * Logs an error if the camera or the VinylSelectController is missing.
         */
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

        /**
         * Checks every frame which record is under the mouse and selects it on left click.
         * Only runs while this spot is active and the state is BrowsingBox.
         */
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

        /**
         * Changes the hovered record and shows its data in the recordInfoUI.
         * Saves the rest position of the record the first time it is hovered.
         *
         * @param vinyl The hovered record, or null if nothing is hovered.
         * @param vinylTransform The transform of the hovered record, or null.
         */
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

        /**
         * Pulls the hovered record out a bit and moves all other records back to their rest position.
         */
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

        /**
         * Immediately snaps every tracked record to its stored rest position.
         * Called on the first frame after returning to the vinyl spot so that
         * colliders and visuals are in sync before raycasting starts.
         */
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

        /**
         * Returns the blockingLayer, or the default raycast layers if blockingLayer is empty.
         *
         * @return The layer mask used for the raycast.
         */
        private int GetBlockingLayerMask()
        {
            return blockingLayer.value != 0
                ? blockingLayer.value
                : Physics.DefaultRaycastLayers;
        }

        /**
         * Checks if a layer is part of a layer mask.
         *
         * @param layer The layer to check.
         * @param layerMask The mask to check against.
         * @return True if the layer is in the mask.
         */
        private static bool IsLayerInMask(int layer, LayerMask layerMask)
        {
            return (layerMask.value & (1 << layer)) != 0;
        }

        /**
         * Searches the hit object and its parents for an IVinyl with vinyl format.
         *
         * @param hit The raycast hit.
         * @param[out] vinylTransform The selection transform of the found record, or null.
         * @return The found record, or null if there is no vinyl.
         */
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
