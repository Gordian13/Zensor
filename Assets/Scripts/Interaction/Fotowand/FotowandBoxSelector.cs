using Core.camera;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Core.FotowandSelect
{
    /// <summary>
    /// Handles mouse hover and click on Fotowand posters (modeled after <c>RecordBoxSelector</c>).
    /// </summary>
    /// <remarks>
    /// Every frame it raycasts from the mouse position and highlights the
    /// <see cref="IFotowand"/> under the cursor. A left click opens <see cref="FotowandUI"/>
    /// with that poster's data.
    ///
    /// It only works while the player stands at <see cref="owningSpot"/>, and only
    /// posters that are children of that spot can be selected.
    ///
    /// While the UI is open, right-click camera orbit and <see cref="SpotInputRaycaster"/>
    /// are disabled so the player cannot move away from the wall.
    ///
    /// Setup: attach to an object under the wall's <see cref="CameraSpot"/> and assign
    /// <see cref="fotowandUI"/>. Camera, SpotManager and SpotInputRaycaster are found
    /// automatically if left empty.
    /// </remarks>
    public class FotowandBoxSelector : MonoBehaviour
    {
        /// <summary>Camera used for the raycast. Defaults to <c>Camera.main</c>.</summary>
        [Header("References")]
        [SerializeField] private Camera targetCamera;
        /// <summary>Info panel opened on click.</summary>
        [SerializeField] private FotowandUI fotowandUI;

        /// <summary>Layers that are checked for posters.</summary>
        [Header("Raycast")]
        [SerializeField] private LayerMask fotowandLayer = ~0;
        /// <summary>Maximum raycast distance.</summary>
        [SerializeField] private float rayDistance = 100f;

        /// <summary>Used to check which spot the player is currently at.</summary>
        [Header("Spot Guard")]
        [SerializeField] private SpotManager spotManager;
        /// <summary>The camera spot of this wall. Defaults to the parent <see cref="CameraSpot"/>.</summary>
        [SerializeField] private CameraSpot owningSpot;

        /// <summary>Spot navigation raycaster that is disabled while the UI is open.</summary>
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

        /// <summary>Moves the highlight to the poster currently under the cursor.</summary>
        private void UpdateHover()
        {
            IFotowand hitFotowand = GetFotowandUnderCursor();

            if (hitFotowand == hoveredFotowand)
                return;

            hoveredFotowand?.SetHighlight(false);
            hoveredFotowand = hitFotowand;
            hoveredFotowand?.SetHighlight(true);
        }

        /// <summary>Removes the highlight from the hovered poster, if any.</summary>
        private void ClearHover()
        {
            if (hoveredFotowand == null)
                return;

            hoveredFotowand.SetHighlight(false);
            hoveredFotowand = null;
        }

        /// <summary>
        /// When the UI opens or closes, disables or re-enables camera orbit and spot navigation.
        /// Runs only on state changes, not every frame.
        /// </summary>
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

        /// <summary>
        /// Raycasts from the mouse position and returns the poster that was hit.
        /// </summary>
        /// <returns>The hit <see cref="IFotowand"/>, or null if nothing was hit or the poster belongs to another spot.</returns>
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

        /// <summary>Checks whether the player is currently at this wall's camera spot.</summary>
        /// <returns>True if at the owning spot, or if no spot guard is configured.</returns>
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