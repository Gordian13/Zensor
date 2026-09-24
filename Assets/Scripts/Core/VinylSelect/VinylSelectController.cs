using System;
using background;
using Core.camera;
using Core.VinylSelect;
using Interaction.util.ColorReveal;
using UnityEngine;

/**
 * @brief Owns the selected vinyl and validates transitions between interaction states.
 *
 * Call transition methods instead of assigning state directly. Successful changes emit
 * StateChanged so VinylSelectionUI and VinylPlayerConnector can update their views.
 * VinylInfoOpen returns to VinylSelected; VinylPlayerInfoOpen returns to vinylPlayer.
 * Opening player information must not be treated as leaving the record player.
 *
 * Requires a parent CameraSpot, GlobalInteractionState.Instance and a selected vinyl
 * with an IColorRevealable child for selection/exit operations. Player entry and exit
 * also use BackGroundMusicManager.Instance. Methods ending in FromButton are void
 * adapters for Unity button events and discard the transition result.
 */
public class VinylSelectController : MonoBehaviour
{
    /** @brief Current interaction state, initially BrowsingBox. */
    public VinylState CurrentVinylState { get; private set; } = VinylState.BrowsingBox;
    /** @brief Selected record provider, or null when no record is selected. */
    public IVinyl SelectedVinyl { get; private set; }

    /**
     * @brief Reports the previous and new state after CurrentVinylState is updated.
     * Selection cleanup order depends on the transition; consumers must tolerate null data.
     */
    public event Action<VinylState, VinylState> StateChanged;
    private CameraSpot _spot;

    public void Awake()
    {
        this._spot = GetComponentInParent<CameraSpot>();
    }

    /**
     * @brief Selects a record from browsing and reserves interaction for inspection.
     * @param vinyl Record provider to select.
     * @return True if selected; false if input is blocked, the state is not BrowsingBox,
     * or vinyl is null.
     */
    public bool SelectVinyl(IVinyl vinyl)
    {
        if (GlobalInteractionState.Instance.IsInteractionBlocked) return false;
        if (CurrentVinylState != VinylState.BrowsingBox || vinyl == null)
            return false;

        GlobalInteractionState.Instance.BlockInteractions();
        this._spot.SetAllowRightClickLook(false);

        SelectedVinyl = vinyl;
        vinyl.GetSelectionTransform().GetComponentInChildren<IColorRevealable>().SetStayColored(true);
        ChangeState(VinylState.VinylSelected);
        return true;
    }

    /**
     * @brief Opens the shared information panel from inspection or player mode.
     * @return True if VinylSelected becomes VinylInfoOpen or vinylPlayer becomes
     * VinylPlayerInfoOpen; false in all other states.
     */
    public bool OpenInfo()
    {
        if (CurrentVinylState == VinylState.VinylSelected)
            return TryChangeState(VinylState.VinylSelected, VinylState.VinylInfoOpen);

        if (CurrentVinylState == VinylState.vinylPlayer)
            return TryChangeState(VinylState.vinylPlayer, VinylState.VinylPlayerInfoOpen);

        return false;
    }

    /**
     * @brief Closes the information panel and restores its corresponding interaction mode.
     * @return True if VinylInfoOpen becomes VinylSelected or VinylPlayerInfoOpen becomes
     * vinylPlayer; false if neither information state is active.
     */
    public bool CloseInfo()
    {
        if (CurrentVinylState == VinylState.VinylInfoOpen)
            return TryChangeState(VinylState.VinylInfoOpen, VinylState.VinylSelected);

        if (CurrentVinylState == VinylState.VinylPlayerInfoOpen)
            return TryChangeState(VinylState.VinylPlayerInfoOpen, VinylState.vinylPlayer);

        return false;
    }

    /**
     * Starts dragging the disc out of its cover.
     */
    public bool BeginDragOut()
    {
        return TryChangeState(VinylState.VinylSelected, VinylState.DraggingVinylOut);
    }

    /**
     * Finishes dragging out and changes to the focused state.
     */
    public bool FinishDragOut()
    {
        return TryChangeState(VinylState.DraggingVinylOut, VinylState.VinylDraggedOutFocused);
    }

    /**
     * Cancels dragging out and returns to the selected state.
     */
    public bool CancelDragOut()
    {
        return TryChangeState(VinylState.DraggingVinylOut, VinylState.VinylSelected);
    }

    /**
     * Starts dragging the focused disc back into its cover.
     */
    public bool BeginDragIn()
    {
        return TryChangeState(VinylState.VinylDraggedOutFocused, VinylState.DraggingVinylIn);
    }

    /**
     * Finishes dragging in and returns to the selected state.
     */
    public bool FinishDragIn()
    {
        return TryChangeState(VinylState.DraggingVinylIn, VinylState.VinylSelected);
    }

    /**
     * Cancels dragging in and returns to the focused state.
     */
    public bool CancelDragIn()
    {
        return TryChangeState(VinylState.DraggingVinylIn, VinylState.VinylDraggedOutFocused);
    }

    /**
     * @brief Releases the selection, color hold and interaction block when leaving inspection.
     * @return True from VinylSelected, VinylInfoOpen or VinylDraggedOutFocused;
     * false during dragging, browsing or player states.
     * @see ExitVinylPlayer
     */
    public bool CloseSelection()
    {
        if (CurrentVinylState != VinylState.VinylSelected &&
            CurrentVinylState != VinylState.VinylInfoOpen &&
            CurrentVinylState != VinylState.VinylDraggedOutFocused)
            return false;
        
        this._spot.SetAllowRightClickLook(true);

        ChangeState(VinylState.BrowsingBox);
        SelectedVinyl.GetSelectionTransform().GetComponentInChildren<IColorRevealable>().SetStayColored(false);
        SelectedVinyl = null;
        GlobalInteractionState.Instance.UnblockInteractions();
        return true;
    }

    // Void wrappers are used because Unity buttons do not display bool-returning methods.
    /** @brief Unity button adapter for OpenInfo(). */
    public void OpenInfoFromButton()
    {
        OpenInfo();
    }

    /** @brief Unity button adapter for CloseInfo(). */
    public void CloseInfoFromButton()
    {
        CloseInfo();
    }

    public void BeginDragOutFromButton()
    {
        BeginDragOut();
    }

    public void FinishDragOutFromButton()
    {
        FinishDragOut();
    }

    public void BeginDragInFromButton()
    {
        BeginDragIn();
    }

    public void FinishDragInFromButton()
    {
        FinishDragIn();
    }

    public void ContinueBrowsingFromButton()
    {
        CloseSelection();
    }

    public void GoToVinylPlayerFromButton()
    {
        GoToVinylPlayer();
    }

    public bool GoToVinylPlayer()
    {
        BackGroundMusicManager.Instance.StopBackGroundMusic();
        if (GlobalPlayedRecordsCounter.Instance != null)
            GlobalPlayedRecordsCounter.Instance.IncreaseRecordsPlayed();
        return TryChangeState(VinylState.VinylDraggedOutFocused, VinylState.vinylPlayer);
    }

    public void ExitVinylPlayerFromButton()
    {
        ExitVinylPlayer();
    }

    /**
     * @brief Returns from either player state to browsing and clears the selected record.
     * @return False outside vinylPlayer/VinylPlayerInfoOpen or while global interactions
     * are blocked; true after restoring background music and camera look control.
     * StateChanged is raised after SelectedVinyl has been cleared.
     */
    public bool ExitVinylPlayer()
    {
        if (CurrentVinylState != VinylState.vinylPlayer &&
            CurrentVinylState != VinylState.VinylPlayerInfoOpen)
            return false;

        if (GlobalInteractionState.Instance.IsInteractionBlocked)
            return false;

        GlobalInteractionState.Instance.UnblockInteractions();
        BackGroundMusicManager.Instance.PlayBackGroundMusic();
        this._spot.SetAllowRightClickLook(true);
        SelectedVinyl.GetSelectionTransform().GetComponentInChildren<IColorRevealable>().SetStayColored(false);
        SelectedVinyl = null;
        ChangeState(VinylState.BrowsingBox);
        return true;
    }

    /**
     * Changes the state only if the controller is currently in the required state.
     */
    private bool TryChangeState(VinylState requiredState, VinylState nextState)
    {
        if (CurrentVinylState != requiredState)
            return false;

        ChangeState(nextState);
        return true;
    }

    /**
     * Stores the new state and informs subscribed view and interaction scripts.
     */
    private void ChangeState(VinylState nextState)
    {
        if (CurrentVinylState == nextState)
            return;

        VinylState previousState = CurrentVinylState;
        CurrentVinylState = nextState;
        StateChanged?.Invoke(previousState, nextState);
    }
}
