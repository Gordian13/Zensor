using Core.VinylSelect;
using UnityEngine;

public class VinylSelectionUI : MonoBehaviour
{
    [SerializeField] private VinylSelectController controller;
    [SerializeField] private RecordInfoUI recordInfoUI;

    [Header("Buttons")]
    [SerializeField] private GameObject infoButton;
    [SerializeField] private GameObject closeInfoButton;
    [SerializeField] private GameObject browseMoreButton;
    [SerializeField] private GameObject playButton;

    public void Configure(
        VinylSelectController selectController,
        RecordInfoUI infoUI,
        GameObject info,
        GameObject closeInfo,
        GameObject browseMore,
        GameObject play)
    {
        controller = selectController;
        recordInfoUI = infoUI;
        infoButton = info;
        closeInfoButton = closeInfo;
        browseMoreButton = browseMore;
        playButton = play;

        Refresh();
    }

    private void OnEnable()
    {
        if (controller == null)
        {
            controller = FindFirstObjectByType<VinylSelectController>();
        }

        if (recordInfoUI == null)
        {
            recordInfoUI = FindFirstObjectByType<RecordInfoUI>(FindObjectsInactive.Include);
        }

        if (controller == null)
        {
            Debug.LogWarning("VinylSelectionUI: VinylSelectController reference is missing.", this);
            return;
        }

        controller.StateChanged += OnStateChanged;
        Refresh();
    }

    private void OnDisable()
    {
        if (controller != null)
        {
            controller.StateChanged -= OnStateChanged;
        }
    }

    private void OnStateChanged(VinylState previousState, VinylState nextState)
    {
        ApplyState(nextState);
    }

    private void Refresh()
    {
        if (controller != null)
        {
            ApplyState(controller.CurrentVinylState);
        }
    }

    private void ApplyState(VinylState state)
    {
        bool isSelected = state == VinylState.Selected;
        bool isInfoOpen = state == VinylState.InfoOpen;
        bool isFocused = state == VinylState.VinylFocused;

        SetVisible(infoButton, isSelected);
        SetVisible(closeInfoButton, isInfoOpen);
        SetVisible(browseMoreButton, isSelected);
        SetVisible(playButton, isFocused);

        if (recordInfoUI == null)
        {
            return;
        }

        if (isInfoOpen)
        {
            recordInfoUI.ShowData(controller.SelectedVinyl?.GetData());
        }
        else
        {
            recordInfoUI.Hide();
        }
    }

    private static void SetVisible(GameObject target, bool visible)
    {
        if (target != null && target.activeSelf != visible)
        {
            target.SetActive(visible);
        }
    }
}
