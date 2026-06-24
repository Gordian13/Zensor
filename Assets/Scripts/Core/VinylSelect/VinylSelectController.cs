using System;
using Core.camera;
using Core.VinylSelect;
using Interaction.util.ColorReveal;
using UnityEngine;

/**
 * Owns the current vinyl selection state and validates all allowed state transitions.
 * Other scripts request changes through this controller instead of changing the state directly.
 */
public class VinylSelectController : MonoBehaviour
{
    public VinylState CurrentVinylState { get; private set; } = VinylState.BrowsingBox;
    public IVinyl SelectedVinyl { get; private set; }

    public event Action<VinylState, VinylState> StateChanged;
    private CameraSpot _spot;

    public void Awake()
    {
        this._spot = GetComponentInParent<CameraSpot>();
    }

    /**
     * Selects a vinyl while browsing and changes the state to VinylSelected.
     */
    public bool SelectVinyl(IVinyl vinyl)
    {
        if (CurrentVinylState != VinylState.BrowsingBox || vinyl == null)
            return false;

        this._spot.SetAllowRightClickLook(false);

        SelectedVinyl = vinyl;
        vinyl.GetSelectionTransform().GetComponentInChildren<IColorRevealable>().SetStayColored(true);
        ChangeState(VinylState.VinylSelected);
        return true;
    }

    /**
     * Opens the information view for the currently selected vinyl.
     */
    public bool OpenInfo()
    {
        return TryChangeState(VinylState.VinylSelected, VinylState.VinylInfoOpen);
    }

    /**
     * Closes the information view and returns to the selected state.
     */
    public bool CloseInfo()
    {
        return TryChangeState(VinylState.VinylInfoOpen, VinylState.VinylSelected);
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
     * Clears the current selection and returns to browsing.
     */
    public bool CloseSelection()
    {
        if (CurrentVinylState != VinylState.VinylSelected)
            return false;
        
        this._spot.SetAllowRightClickLook(true);

        ChangeState(VinylState.BrowsingBox);
        SelectedVinyl.GetSelectionTransform().GetComponentInChildren<IColorRevealable>().SetStayColored(false);
        SelectedVinyl = null;
        return true;
    }

    // Void wrappers are used because Unity buttons do not display bool-returning methods.
    public void OpenInfoFromButton()
    {
        OpenInfo();
    }

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
        return TryChangeState(VinylState.VinylDraggedOutFocused, VinylState.vinylPlayer);
    }

    public void ExitVinylPlayerFromButton()
    {
        ExitVinylPlayer();
    }

    public bool ExitVinylPlayer()
    {
        if (CurrentVinylState != VinylState.vinylPlayer)
            return false;

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
