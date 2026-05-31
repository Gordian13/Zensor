using UnityEngine;
using UnityEngine.InputSystem;

namespace Core.camera
{
    public class SpotInputRaycaster : MonoBehaviour
    {
        [SerializeField] private Camera targetCamera;
        [SerializeField] private SpotManager spotManager;
        [SerializeField] private CameraSpotRegistry registry;
        [SerializeField] private CameraTransitionManager transitionManager;
        [SerializeField] private LayerMask spotLayer = ~0;
        [SerializeField] private float rayDistance = 100f;

        private SpotNavigationTrigger currentHoveredTrigger;

        private void Awake()
        {
            if (targetCamera == null)
                Debug.LogError($"{nameof(SpotInputRaycaster)} has no target camera assigned.", this);

            if (spotManager == null)
                Debug.LogError($"{nameof(SpotInputRaycaster)} has no SpotManager assigned.", this);

            if (registry == null)
                Debug.LogError($"{nameof(SpotInputRaycaster)} has no CameraSpotRegistry assigned.", this);

            if (transitionManager == null)
                Debug.LogError($"{nameof(SpotInputRaycaster)} has no CameraTransitionManager assigned.", this);
        }

        private void Update()
        {
            if (transitionManager != null && transitionManager.IsTransitioning)
                return;

            UpdateHover();

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                ClickHoveredSpot();
        }

        private void UpdateHover()
        {
            SpotNavigationTrigger newHoveredTrigger = RaycastTrigger();

            if (newHoveredTrigger == currentHoveredTrigger)
                return;

            SetHoveredReveal(false);
            currentHoveredTrigger = newHoveredTrigger;
            SetHoveredReveal(true);
        }

        private void ClickHoveredSpot()
        {
            if (currentHoveredTrigger == null)
                return;

            if (registry == null || spotManager == null || transitionManager == null)
            {
                Debug.LogError($"{nameof(SpotInputRaycaster)} cannot click because required references are missing.", this);
                return;
            }

            string targetSpotId = currentHoveredTrigger.GetTargetSpotId();
            if (string.IsNullOrWhiteSpace(targetSpotId))
            {
                Debug.LogError("Clicked spot trigger has no target spot assigned.", currentHoveredTrigger);
                return;
            }

            CameraSpot targetSpot = registry.GetSpot(targetSpotId);

            if (targetSpot == null || spotManager.IsCurrentSpot(targetSpot))
            {
                Debug.LogWarning($"Clicked route target '{targetSpotId}' was not found or is already current.", currentHoveredTrigger);
                return;
            }

            CameraRoute route = GetCurrentRoute();
            if (route == null)
            {
                Debug.LogWarning("Clicked spot trigger, but no route matched the current spot.", currentHoveredTrigger);
                return;
            }

            targetSpot.SetSpotReveal(false);
            transitionManager.PlayRoute(route, targetSpotId);
        }

        private void SetHoveredReveal(bool isReveal)
        {
            if (currentHoveredTrigger == null || registry == null || spotManager == null)
                return;

            string targetSpotId = currentHoveredTrigger.GetTargetSpotId();
            if (string.IsNullOrWhiteSpace(targetSpotId))
            {
                Debug.LogError("Hovered spot trigger has no target spot assigned.", currentHoveredTrigger);
                return;
            }

            CameraSpot targetSpot = registry.GetSpot(targetSpotId);

            if (targetSpot == null || spotManager.IsCurrentSpot(targetSpot))
                return;

            targetSpot.SetSpotReveal(isReveal);
        }

        private SpotNavigationTrigger RaycastTrigger()
        {
            if (targetCamera == null || Mouse.current == null)
                return null;

            Ray ray = targetCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (!Physics.Raycast(ray, out RaycastHit hit, rayDistance, spotLayer))
                return null;

            return hit.collider.GetComponentInParent<SpotNavigationTrigger>();
        }

        private CameraRoute GetCurrentRoute()
        {
            if (currentHoveredTrigger == null || spotManager == null)
            {
                Debug.LogError($"{nameof(SpotInputRaycaster)} cannot get route because current trigger or SpotManager is missing.", this);
                return null;
            }

            return currentHoveredTrigger.GetRouteFrom(spotManager.GetCurrentSpotId());
        }
    }
}
