using Core.VinylSelect;
using TMPro;
using UnityEngine;

/**
 * Updates the vinyl UI whenever the selection state changes.
 * Controls the visible buttons and displays the selected record information.
 */
public class VinylSelectionUI : MonoBehaviour
{
    [SerializeField] private VinylSelectController controller;
    [SerializeField] private RecordInfoUI recordInfoUI;

    [Header("Buttons")]
    [SerializeField] private GameObject infoButton;
    [SerializeField] private GameObject closeInfoButton;
    [SerializeField] private GameObject browseMoreButton;
    [SerializeField] private GameObject playButton;

    [Header("Guidance")]
    [SerializeField] private GameObject guidanceRoot;
    [SerializeField] private TMP_Text primaryHintText;
    [SerializeField] private TMP_Text secondaryHintText;

    [SerializeField] private string pullRecordOutHint = "Left Mouse Button + Drag -> Pull Record Out";
    [SerializeField] private string putRecordBackHint = "Left Mouse Button + Drag -> Put Record Back";
    [SerializeField] private string rotateHint = "Hold Right Mouse Button -> Rotate Cover or Record";

    /**
     * Connects the controller, info panel, and buttons when the UI is created by the editor setup tool.
     */
    public void Configure(
        VinylSelectController selectController,
        RecordInfoUI infoUI,
        GameObject info,
        GameObject closeInfo,
        GameObject browseMore,
        GameObject play,
        GameObject guidance = null,
        TMP_Text primaryHint = null,
        TMP_Text secondaryHint = null)
    {
        controller = selectController;
        recordInfoUI = infoUI;
        infoButton = info;
        closeInfoButton = closeInfo;
        browseMoreButton = browseMore;
        playButton = play;
        guidanceRoot = guidance;
        primaryHintText = primaryHint;
        secondaryHintText = secondaryHint;

        Refresh();
    }

    /**
     * Finds missing references, subscribes to state changes, and applies the current state.
     */
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

    /**
     * Removes the state event subscription when this component is disabled.
     */
    private void OnDisable()
    {
        if (controller != null)
        {
            controller.StateChanged -= OnStateChanged;
        }
    }

    /**
     * Applies the new state whenever the VinylSelectController reports a change.
     */
    private void OnStateChanged(VinylState previousState, VinylState nextState)
    {
        ApplyState(nextState);
    }

    /**
     * Refreshes the UI from the controller's current state.
     */
    private void Refresh()
    {
        if (controller != null)
        {
            ApplyState(controller.CurrentVinylState);
        }
    }

    /**
     * Shows the buttons and information panel required by the given vinyl state.
     */
    private void ApplyState(VinylState state)
    {
        bool isSelected = state == VinylState.VinylSelected;
        bool isInfoOpen = state == VinylState.VinylInfoOpen;
        bool isFocused = state == VinylState.VinylDraggedOutFocused;

        SetVisible(infoButton, isSelected);
        SetVisible(closeInfoButton, isInfoOpen);
        SetVisible(browseMoreButton, isSelected);
        SetVisible(playButton, isFocused);
        UpdateGuidance(state);

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

    /**
     * Shows contextual control hints while the player can manipulate the selected vinyl.
     */
    private void UpdateGuidance(VinylState state)
    {
        bool canPullRecordOut = state == VinylState.VinylSelected ||
                                state == VinylState.DraggingVinylOut;
        bool canPutRecordBack = state == VinylState.VinylDraggedOutFocused ||
                                state == VinylState.DraggingVinylIn;
        bool shouldShowGuidance = canPullRecordOut || canPutRecordBack;

        SetVisible(guidanceRoot, shouldShowGuidance);

        if (!shouldShowGuidance)
        {
            return;
        }

        if (primaryHintText != null)
        {
            primaryHintText.text = canPutRecordBack ? putRecordBackHint : pullRecordOutHint;
        }

        if (secondaryHintText != null)
        {
            secondaryHintText.text = rotateHint;
        }
    }

    /**
     * Activates or deactivates a UI object when its visibility needs to change.
     */
    private static void SetVisible(GameObject target, bool visible)
    {
        if (target != null && target.activeSelf != visible)
        {
            target.SetActive(visible);
        }
    }
}
