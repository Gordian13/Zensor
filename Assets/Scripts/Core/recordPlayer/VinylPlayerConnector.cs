using Core.camera;
using Core.VinylSelect;
using UnityEngine;
using UnityEngine.InputSystem;

namespace recordPlayer
{
    /**
     * Bridges the vinyl selection flow with the record player.
     * Uses direct CameraSpot references to avoid cross-scene registry lookup issues.
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

            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                ExitPlayer();
        }

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

        private static bool IsPlayerState(VinylState state)
        {
            return state == VinylState.vinylPlayer ||
                   state == VinylState.VinylPlayerInfoOpen;
        }
    }
}
