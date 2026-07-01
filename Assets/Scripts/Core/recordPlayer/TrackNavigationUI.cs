using Core.VinylSelect;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace recordPlayer
{
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
            bool isPlayer = next == VinylState.vinylPlayer;
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
    }
}
