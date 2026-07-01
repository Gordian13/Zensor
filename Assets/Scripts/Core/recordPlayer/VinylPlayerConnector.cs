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
            if (selectController.CurrentVinylState != VinylState.vinylPlayer) return;

            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                ExitPlayer();
        }

        private void OnStateChanged(VinylState previous, VinylState next)
        {
            if (next == VinylState.vinylPlayer)
                EnterPlayer();
            else if (previous == VinylState.vinylPlayer)
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
            recordPlayer?.ClearRecord();

            if (browsingSpot != null)
                CameraTransitionManager.Instance?.PlayRoute(routeToBrowsing, browsingSpot);

            selectController?.ExitVinylPlayer();
        }
    }
}
