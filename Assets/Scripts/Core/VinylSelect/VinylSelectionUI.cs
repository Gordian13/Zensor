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
    [SerializeField] private GameObject nextTrackButton;
    [SerializeField] private GameObject previousTrackButton;

    [Header("Guidance")]
    [SerializeField] private GameObject guidanceRoot;
    [SerializeField] private TMP_Text primaryHintText;
    [SerializeField] private TMP_Text secondaryHintText;

    [SerializeField] private string pullRecordOutHint = "Linke Maustaste + Ziehen -> Platte herausziehen";
    [SerializeField] private string putRecordBackHint = "Linke Maustaste + Ziehen -> Platte zurücklegen";
    [SerializeField] private string rotateHint = "Rechte Maustaste halten + Maus bewegen -> Cover oder Platte drehen";
    [SerializeField] private string playerTrackHint = "Weiter / Zurück -> Titel wechseln";
    [SerializeField] private string playerInfoHint = "Info -> Details anzeigen | Zurück -> zur Plattenauswahl";

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

        ResolveRecordInfoUI();

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
        bool isInPlayer = state == VinylState.vinylPlayer;
        bool isPlayerInfoOpen = state == VinylState.VinylPlayerInfoOpen;
        bool isAnyInfoOpen = isInfoOpen || isPlayerInfoOpen;

        bool hasMultipleTracks = controller.SelectedVinyl?.GetData()?.TrackCount > 1;

        ResolveRecordInfoUI();

        SetVisible(infoButton, isSelected || isInPlayer);
        SetVisible(closeInfoButton, isAnyInfoOpen);
        SetVisible(browseMoreButton, isSelected);
        SetVisible(playButton, isFocused);
        SetVisible(nextTrackButton, isInPlayer && hasMultipleTracks);
        SetVisible(previousTrackButton, isInPlayer && hasMultipleTracks);
        UpdateGuidance(state);

        if (recordInfoUI == null)
        {
            if (isAnyInfoOpen)
            {
                Debug.LogWarning("VinylSelectionUI: RecordInfoUI reference is missing.", this);
            }

            return;
        }

        if (isAnyInfoOpen)
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
        bool isInPlayer = state == VinylState.vinylPlayer;
        bool shouldShowGuidance = canPullRecordOut || canPutRecordBack || isInPlayer;

        SetVisible(guidanceRoot, shouldShowGuidance);

        if (!shouldShowGuidance)
        {
            return;
        }

        if (isInPlayer)
        {
            if (primaryHintText != null)
            {
                primaryHintText.text = playerTrackHint;
            }

            if (secondaryHintText != null)
            {
                secondaryHintText.text = playerInfoHint;
            }

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

    private void ResolveRecordInfoUI()
    {
        if (recordInfoUI == null)
        {
            recordInfoUI = FindFirstObjectByType<RecordInfoUI>(FindObjectsInactive.Include);
        }
    }
}
