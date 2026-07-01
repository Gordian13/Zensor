using System.Collections.Generic;
using Core.camera;
using Core.VinylSelect;
using Interaction.util.ColorReveal;
using UnityEngine;
using UnityEngine.InputSystem;

namespace record
{
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
            if (!IsOwningSpotActive() ||
                vinylSelectController == null ||
                vinylSelectController.CurrentVinylState != VinylState.BrowsingBox)
            {
                ClearHover(true);
                return;
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
                    targetPosition += hoverOffset;

                record.Key.localPosition = Vector3.Lerp(
                    record.Key.localPosition,
                    targetPosition,
                    moveSpeed * Time.deltaTime
                );

                if (Vector3.SqrMagnitude(record.Key.localPosition - targetPosition) < 0.000001f)
                    record.Key.localPosition = targetPosition;
            }
        }

        private IVinyl GetVinylUnderCursor(out Transform vinylTransform)
        {
            vinylTransform = null;

            Mouse mouse = Mouse.current;
            if (targetCamera == null || mouse == null)
                return null;

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
