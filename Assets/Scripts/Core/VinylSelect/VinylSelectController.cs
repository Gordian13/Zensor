using System;
using Core.VinylSelect;
using UnityEngine;

public class VinylSelectController : MonoBehaviour
{
    public VinylState CurrentVinylState { get; private set; } = VinylState.Browsing;
    public IVinyl SelectedVinyl { get; private set; }

    public event Action<VinylState, VinylState> StateChanged;
    
    public bool SelectVinyl(IVinyl vinyl)
    {
        if (CurrentVinylState != VinylState.Browsing || vinyl == null)
            return false;

        SelectedVinyl = vinyl;
        ChangeState(VinylState.Selected);
        return true;
    }

    public bool OpenInfo()
    {
        return TryChangeState(VinylState.Selected, VinylState.InfoOpen);
    }

    public bool CloseInfo()
    {
        return TryChangeState(VinylState.InfoOpen, VinylState.Selected);
    }

    public bool BeginDragOut()
    {
        return TryChangeState(VinylState.Selected, VinylState.DraggingOut);
    }

    public bool FinishDragOut()
    {
        return TryChangeState(VinylState.DraggingOut, VinylState.VinylFocused);
    }

    public bool BeginDragIn()
    {
        return TryChangeState(VinylState.VinylFocused, VinylState.DraggingIn);
    }

    public bool FinishDragIn()
    {
        return TryChangeState(VinylState.DraggingIn, VinylState.Selected);
    }

    public bool CloseSelection()
    {
        if (CurrentVinylState != VinylState.Selected)
            return false;

        ChangeState(VinylState.Browsing);
        SelectedVinyl = null;
        return true;
    }

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

    private bool TryChangeState(VinylState requiredState, VinylState nextState)
    {
        if (CurrentVinylState != requiredState)
            return false;

        ChangeState(nextState);
        return true;
    }

    private void ChangeState(VinylState nextState)
    {
        if (CurrentVinylState == nextState)
            return;

        VinylState previousState = CurrentVinylState;
        CurrentVinylState = nextState;
        StateChanged?.Invoke(previousState, nextState);
    }
}
