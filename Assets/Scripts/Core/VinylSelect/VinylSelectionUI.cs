using Core.VinylSelect;
using TMPro;
using UnityEngine;

/**
 * @brief Synchronizes vinyl buttons, hints and metadata with the selection state.
 *
 * Subscribes to VinylSelectController.StateChanged while enabled. Assign controller,
 * recordInfoUI and the relevant button GameObjects in the Inspector or via Configure().
 * A missing controller is searched for on enable; panel lookup includes inactive objects.
 *
 * VinylInfoOpen and VinylPlayerInfoOpen share the same RecordInfoUI. The Info button
 * is available in VinylSelected and vinylPlayer, while Play requires a focused disc.
 * Track buttons require the normal player state and more than one track. Optional
 * guidance fields display dragging and rotation hints only during inspection/dragging.
 * This component controls visibility and text, not the buttons' onClick bindings.
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

    /**
     * @brief Assigns UI references and immediately refreshes from the controller state.
     * @param selectController Controller supplying the current selection and state.
     * @param infoUI Shared metadata panel.
     * @param info Button object for opening information.
     * @param closeInfo Button object for closing information.
     * @param browseMore Button object for returning to browsing.
     * @param play Button object for entering the player.
     * @param guidance Optional parent object for inspection hints; null disables this reference.
     * @param primaryHint Optional text for the current dragging action.
     * @param secondaryHint Optional text for the rotation gesture.
     *
     * Track button references remain Inspector-assigned. This method does not rebind
     * StateChanged subscriptions; configure the controller before enabling the component.
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
     * @brief Applies visibility rules and displays metadata for either information state.
     * @param state Current state reported by the selection controller.
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

    /** @brief Finds an unassigned metadata panel, including panels that are currently inactive. */
    private void ResolveRecordInfoUI()
    {
        if (recordInfoUI == null)
        {
            recordInfoUI = FindFirstObjectByType<RecordInfoUI>(FindObjectsInactive.Include);
        }
    }
}
