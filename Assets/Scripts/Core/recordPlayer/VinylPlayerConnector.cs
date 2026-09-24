using Core.camera;
using Core.VinylSelect;
using UnityEngine;
using UnityEngine.InputSystem;

namespace recordPlayer
{
    /**
     * @brief Connects selection state changes to record loading and player camera routes.
     *
     * Assign recordPlayer, playerSpot, browsingSpot and the desired routes in the Inspector.
     * selectController is searched for when the component is enabled, and the scene must
     * provide CameraTransitionManager.Instance for entering the player view.
     *
     * vinylPlayer and VinylPlayerInfoOpen are treated as one player session. Switching
     * between them only changes the information UI; it must not reload/clear the record
     * or restart camera navigation. The state subscription is removed on disable.
     */
    public class VinylPlayerConnector : MonoBehaviour
    {
        [Header("Assign in Inspector")]
        [SerializeField] private RecordPlayerController recordPlayer;
        [SerializeField] private CameraSpot playerSpot;
        [SerializeField] private CameraSpot browsingSpot;
        [SerializeField] private CameraRoute routeToPlayer;
        [SerializeField] private CameraRoute routeToBrowsing;

        [Header("Auto-found at runtime - leave empty")]
        [SerializeField] private VinylSelectController selectController;

        private bool _isExiting;

        private void OnEnable()
        {
            if (selectController == null)
                selectController = FindFirstObjectByType<VinylSelectController>();

            if (selectController != null)
                selectController.StateChanged += OnStateChanged;
        }

        private void OnDisable()
        {
            if (selectController != null)
                selectController.StateChanged -= OnStateChanged;
        }

        private void Update()
        {
            if (selectController == null) return;
            if (!IsPlayerState(selectController.CurrentVinylState)) return;
        }

        /**
         * @brief Handles transitions across the player-session boundary only.
         * @param previous State before the change.
         * @param next State after the change.
         */
        private void OnStateChanged(VinylState previous, VinylState next)
        {
            bool enteredPlayer = !IsPlayerState(previous) && IsPlayerState(next);
            bool exitedPlayer = IsPlayerState(previous) && !IsPlayerState(next);

            if (enteredPlayer)
                EnterPlayer();
            else if (exitedPlayer)
                ExitPlayer();
        }

        private void EnterPlayer()
        {
            IVinyl selectedVinyl = selectController.SelectedVinyl;
            if (selectedVinyl != null && recordPlayer != null)
                recordPlayer.SetRecord(selectedVinyl.GetData());

            if (CameraTransitionManager.Instance == null)
            {
                Debug.LogError("VinylPlayerConnector: CameraTransitionManager nicht gefunden!", this);
                return;
            }

            if (playerSpot == null)
            {
                Debug.LogError("VinylPlayerConnector: Player Spot nicht zugewiesen!", this);
                return;
            }

            CameraTransitionManager.Instance.PlayRoute(routeToPlayer, playerSpot);
        }

        /**
         * @brief Clears playback, requests the browsing route and asks the controller to exit.
         *
         * A reentrancy guard prevents nested exit calls triggered by StateChanged.
         * Record cleanup and routing occur before ExitVinylPlayer() is called; its
         * boolean result is not checked here. The controller may reject a blocked exit.
         */
        public void ExitPlayer()
        {
            if (_isExiting)
                return;

            _isExiting = true;
            recordPlayer?.ClearRecord();

            if (browsingSpot != null)
                CameraTransitionManager.Instance?.PlayRoute(routeToBrowsing, browsingSpot);

            selectController?.ExitVinylPlayer();
            _isExiting = false;
        }

        /**
         * @brief Treats the player metadata overlay as part of the active player session.
         * @param state State to classify.
         * @return True for vinylPlayer or VinylPlayerInfoOpen.
         */
        private static bool IsPlayerState(VinylState state)
        {
            return state == VinylState.vinylPlayer ||
                   state == VinylState.VinylPlayerInfoOpen;
        }
    }
}
