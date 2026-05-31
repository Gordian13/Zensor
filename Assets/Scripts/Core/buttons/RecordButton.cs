using UnityEngine;
using recordPlayer;

public enum ButtonType { StartStop, Pitch33, Pitch45 }

public class RecordButton : MonoBehaviour
{
    public ButtonType buttonType;
    public RecordPlayerController controller;

    public void OnPress()
    {
        switch (buttonType)
        {
            case ButtonType.StartStop: controller.TogglePlay();   break;
            case ButtonType.Pitch33:  controller.SetPitch(33);   break;
            case ButtonType.Pitch45:  controller.SetPitch(45);   break;
        }
    }
}