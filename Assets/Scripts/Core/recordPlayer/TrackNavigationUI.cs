using Core.VinylSelect;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace recordPlayer
{
    /**
     * @brief Connects track navigation controls and updates the current track label.
     *
     * Assign the button and text references in the Inspector. Start finds the record
     * player and selection controller and binds their actions. Both vinylPlayer and
     * VinylPlayerInfoOpen keep the track label and Back button visible, so opening
     * metadata does not remove the exit control. VinylSelectionUI separately controls
     * next/previous button visibility according to state and track count.
     */
    public class TrackNavigationUI : MonoBehaviour
    {
        [SerializeField] private Button nextTrackButton;
        [SerializeField] private Button previousTrackButton;
        [SerializeField] private Button BackButton;
        [SerializeField] private TMP_Text trackDisplay;

        private RecordPlayerController _player;
        private VinylSelectController _selectController;
        private int _lastTrackIndex = -1;

        private void Start()
        {
            _player = FindFirstObjectByType<RecordPlayerController>();
            _selectController = FindFirstObjectByType<VinylSelectController>();

            if (_player == null)
            {
                Debug.LogError("TrackNavigationUI: RecordPlayerController nicht gefunden.", this);
                return;
            }

            nextTrackButton?.onClick.AddListener(_player.NextTrack);
            previousTrackButton?.onClick.AddListener(_player.PreviousTrack);
            BackButton?.onClick.AddListener(() => _selectController?.ExitVinylPlayer()); 

            if (_selectController != null)
                _selectController.StateChanged += OnStateChanged;

            SetVisible(false);
        }

        private void OnDestroy()
        {
            if (_selectController != null)
                _selectController.StateChanged -= OnStateChanged;
        }

        private void OnStateChanged(VinylState previous, VinylState next)
        {
            bool isPlayer = IsPlayerState(next);
            if (isPlayer) _lastTrackIndex = -1;
            SetVisible(isPlayer);
        }

        private void Update()
        {
            if (_player == null || trackDisplay == null) return;
            if (_player.CurrentTrackIndex == _lastTrackIndex) return;

            _lastTrackIndex = _player.CurrentTrackIndex;
            trackDisplay.text = $"{_player.CurrentTrackIndex + 1} / {_player.TrackCount}  —  {_player.CurrentTrackName}";
        }

        private void SetVisible(bool visible)
        {
            if (trackDisplay != null) trackDisplay.gameObject.SetActive(visible);
            if (BackButton != null) BackButton.gameObject.SetActive(visible);
        }

        /**
         * @brief Includes the information overlay when checking for player controls.
         * @param state State to classify.
         * @return True for normal player mode or its metadata overlay.
         */
        private static bool IsPlayerState(VinylState state)
        {
            return state == VinylState.vinylPlayer ||
                   state == VinylState.VinylPlayerInfoOpen;
        }
    }
}
