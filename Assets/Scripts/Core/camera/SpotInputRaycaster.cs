using Core.VinylSelect;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Core.camera
{
    /**
     * @brief Resolves navigation targets under the mouse and requests camera transitions.
     *
     * The vinyl integration suppresses navigation in both vinylPlayer and
     * VinylPlayerInfoOpen, keeping clicks in the player view from selecting room spots.
     * A missing vinylSelectController is searched for, including inactive objects.
     * UI hover, camera transitions, global interaction blocks and an open photo wall
     * also prevent navigation updates.
     */
    public class SpotInputRaycaster : MonoBehaviour
    {
        /** The camera the raycast starts from. */
        [SerializeField] private Camera targetCamera;
        /** Knows the current spot. */
        [SerializeField] private SpotManager spotManager;
        /** Used to find the target spots by id. */
        [SerializeField] private CameraSpotRegistry registry;
        /** Plays the transition to the clicked spot. */
        [SerializeField] private CameraTransitionManager transitionManager;
        /** Used to block navigation while the record player is open. */
        [SerializeField] private VinylSelectController vinylSelectController;
        /** Layers of the spot triggers. */
        [SerializeField] private LayerMask spotLayer = ~0;
        /** Layers the raycast hits, so walls block the ray. */
        [SerializeField] private LayerMask wallBlockLayer = ~0;
        /** How far the raycast reaches (in meters). */
        [SerializeField] private float rayDistance = 100f;
        /** Used to block navigation while the photo wall is open. */
        [SerializeField] private FotowandUI fotowandUI;

        /** The trigger under the mouse right now, or null. */
        private SpotNavigationTrigger currentHoveredTrigger;

        /** Logs an error for every missing reference. */
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

        /**
         * Checks every frame which trigger is under the mouse and starts the transition on left click.
         * Does nothing while the mouse is over UI, a transition runs, interactions are blocked,
         * the record player or the photo wall is open.
         */
        private void Update()
        {
            // https://discussions.unity.com/t/how-to-stop-raycast-by-ui/915538/14

            if (IsPointerOverUI())
            {
                ClearHover();
                return;
            }

            if (GlobalInteractionState.Instance.IsInteractionBlocked)
                return;

            if (transitionManager != null && transitionManager.IsTransitioning)
                return;

            // Another interaction (dialogue, vinyl flow, ...) currently owns the input.
            if (GlobalInteractionState.Instance != null &&
                GlobalInteractionState.Instance.IsInteractionBlocked)
                return;

            if (vinylSelectController == null)
                vinylSelectController = FindFirstObjectByType<VinylSelectController>(FindObjectsInactive.Include);

            if (vinylSelectController != null && IsVinylPlayerState(vinylSelectController.CurrentVinylState))
                return;

            if (fotowandUI == null)
                fotowandUI = FindFirstObjectByType<FotowandUI>();

            if (fotowandUI != null && fotowandUI.IsOpen)
                return;

            UpdateHover();

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                ClickHoveredSpot();
        }

        /** Changes the hovered trigger and reveals the spot it leads to. */
        private void UpdateHover()
        {
            SpotNavigationTrigger newHoveredTrigger = RaycastTrigger();

            if (newHoveredTrigger == currentHoveredTrigger)
                return;

            SetHoveredReveal(false);
            currentHoveredTrigger = newHoveredTrigger;
            SetHoveredReveal(true);
        }

        /** Starts the transition to the spot of the hovered trigger. */
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

            // No matching route means a direct blend to the target spot.
            CameraRoute route = GetCurrentRoute();

            targetSpot.SetSpotReveal(false);
            transitionManager.PlayRoute(route, targetSpotId);
        }

        /**
         * Reveals or hides the target spot of the hovered trigger, but not if it is the current spot.
         *
         * @param isReveal True to show the spot in color.
         */
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

        /**
         * Shoots a ray from the mouse and checks if the first hit is a spot trigger.
         *
         * @return The hit trigger, or null if the first hit is something else.
         */
        private SpotNavigationTrigger RaycastTrigger()
        {
            if (targetCamera == null || Mouse.current == null)
                return null;

            Ray ray = targetCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

            // Use wallBlockLayer (all layers by default) so walls block the ray.
            // Only return a trigger if the very first hit object is a spot trigger.
            if (!Physics.Raycast(ray, out RaycastHit hit, rayDistance, wallBlockLayer))
                return null;

            return hit.collider.GetComponentInParent<SpotNavigationTrigger>();
        }

        /** @return The route from the current spot to the hovered trigger, or null for a direct blend. */
        private CameraRoute GetCurrentRoute()
        {
            if (currentHoveredTrigger == null || spotManager == null)
            {
                Debug.LogError($"{nameof(SpotInputRaycaster)} cannot get route because current trigger or SpotManager is missing.", this);
                return null;
            }

            return currentHoveredTrigger.GetRouteFrom(spotManager.GetCurrentSpotId());
        }

        /**
         * @brief Includes the metadata overlay in the player navigation lock.
         * @param state State to classify.
         * @return True while the normal player view or its information panel is active.
         */
        private static bool IsVinylPlayerState(VinylState state)
        {
            return state == VinylState.vinylPlayer ||
                   state == VinylState.VinylPlayerInfoOpen;
        }


        /** Hides the target spot and forgets the hovered trigger. */
        private void ClearHover()
        {
            if (currentHoveredTrigger == null)
                return;

            SetHoveredReveal(false);
            currentHoveredTrigger = null;
        }

        /** @return True if the mouse is over a UI element. */
        private static bool IsPointerOverUI()
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }
    }

    

}
