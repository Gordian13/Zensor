using Core.VinylSelect;
using UnityEngine;

namespace Core.camera
{
    /**
     * @brief Connects the return-to-start UI action to the camera navigation system.
     *
     * Assign spotManager and transitionManager or let Awake find them in the loaded scene.
     * Wire the button's onClick event to ReturnToStart(). The target comes from
     * SpotManager.GetStartSpotId(); this component requests a route instead of moving
     * the camera directly. The scene must provide GlobalInteractionState.Instance.
     */
    public class ReturnToStartButton : MonoBehaviour
    {
        [SerializeField] private SpotManager spotManager;
        [SerializeField] private CameraTransitionManager transitionManager;

        private void Awake()
        {
            if (spotManager == null)
                spotManager = FindFirstObjectByType<SpotManager>();

            if (transitionManager == null)
                transitionManager = FindFirstObjectByType<CameraTransitionManager>();
        }

        /**
         * @brief Requests navigation to the configured start spot when interaction is allowed.
         *
         * Returns without navigating if a required manager or start ID is missing, or
         * global interactions are blocked. Otherwise requests closure of an active vinyl
         * selection/player state and calls PlayRoute with no explicit route asset.
         * Closure return values are not checked by this method.
         */
        public void ReturnToStart()
        {
            if (spotManager == null)
            {
                Debug.LogError($"{nameof(ReturnToStartButton)} has no SpotManager assigned.", this);
                return;
            }

            if (transitionManager == null)
            {
                Debug.LogError($"{nameof(ReturnToStartButton)} has no CameraTransitionManager assigned.", this);
                return;
            }

            if (GlobalInteractionState.Instance.IsInteractionBlocked)
                return;

            string startSpotId = spotManager.GetStartSpotId();
            if (string.IsNullOrWhiteSpace(startSpotId))
            {
                Debug.LogError($"{nameof(ReturnToStartButton)} cannot return because no start spot id was found.", this);
                return;
            }
            
            VinylSelectController vinylController = FindFirstObjectByType<VinylSelectController>();
            if (vinylController != null)
            {
                if (IsVinylPlayerState(vinylController.CurrentVinylState))
                    vinylController.ExitVinylPlayer();
                else if (vinylController.CurrentVinylState != VinylState.BrowsingBox)
                    vinylController.CloseSelection();
            }

            transitionManager.PlayRoute(null, startSpotId);
            
        }

        private static bool IsVinylPlayerState(VinylState state)
        {
            return state == VinylState.vinylPlayer ||
                   state == VinylState.VinylPlayerInfoOpen;
        }
    }
}
