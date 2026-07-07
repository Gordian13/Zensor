using Core.camera;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Core.FotowandSelect
{
    public class FotowandBoxSelector : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Camera targetCamera;
        [SerializeField] private FotowandUI fotowandUI;

        [Header("Raycast")]
        [SerializeField] private LayerMask fotowandLayer = ~0;
        [SerializeField] private float rayDistance = 100f;

        [Header("Spot Guard")]
        [SerializeField] private SpotManager spotManager;
        [SerializeField] private CameraSpot owningSpot;

        [Header("Global Navigation Lock")]
        [SerializeField] private SpotInputRaycaster spotInputRaycaster;

        private bool wasUiOpen;
        private IFotowand hoveredFotowand;

        private void Awake()
        {
            if (targetCamera == null)
                targetCamera = Camera.main;

            if (spotManager == null)
                spotManager = FindFirstObjectByType<SpotManager>();

            if (owningSpot == null)
                owningSpot = GetComponentInParent<CameraSpot>();

            if (spotInputRaycaster == null)
                spotInputRaycaster = FindFirstObjectByType<SpotInputRaycaster>();
        }

        private void Update()
        {
            if (!IsOwningSpotActive())
            {
                ClearHover();
                return;
            }

            HandleUiStateChange();

            if (fotowandUI != null && fotowandUI.IsOpen)
            {
                ClearHover();
                return;
            }

            UpdateHover();

            Mouse mouse = Mouse.current;
            if (mouse == null || !mouse.leftButton.wasPressedThisFrame)
                return;

            if (hoveredFotowand != null)
                fotowandUI.Open(hoveredFotowand.GetData());
        }

        private void UpdateHover()
        {
            IFotowand hitFotowand = GetFotowandUnderCursor();

            if (hitFotowand == hoveredFotowand)
                return;

            hoveredFotowand?.SetHighlight(false);
            hoveredFotowand = hitFotowand;
            hoveredFotowand?.SetHighlight(true);
        }

        private void ClearHover()
        {
            if (hoveredFotowand == null)
                return;

            hoveredFotowand.SetHighlight(false);
            hoveredFotowand = null;
        }

        private void HandleUiStateChange()
        {
            if (fotowandUI == null || owningSpot == null)
                return;

            bool isUiOpenNow = fotowandUI.IsOpen;
            if (isUiOpenNow == wasUiOpen)
                return;

            wasUiOpen = isUiOpenNow;
            owningSpot.SetAllowRightClickLook(!isUiOpenNow);

            if (spotInputRaycaster != null)
                spotInputRaycaster.enabled = !isUiOpenNow;
        }

        private IFotowand GetFotowandUnderCursor()
        {
            if (targetCamera == null)
                targetCamera = Camera.main;

            if (targetCamera == null || Mouse.current == null)
                return null;

            Ray ray = targetCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (!Physics.Raycast(ray, out RaycastHit hitInfo, rayDistance, fotowandLayer))
                return null;

            IFotowand hit = hitInfo.collider.GetComponentInParent<IFotowand>();

            if (hit is MonoBehaviour hitBehaviour && owningSpot != null)
            {
                if (!hitBehaviour.transform.IsChildOf(owningSpot.transform))
                    return null;
            }

            return hit;
        }

        private bool IsOwningSpotActive()
        {
            if (spotManager == null)
                spotManager = FindFirstObjectByType<SpotManager>();

            if (spotManager == null || owningSpot == null)
                return true;

            return spotManager.IsCurrentSpot(owningSpot);
        }
    }
}