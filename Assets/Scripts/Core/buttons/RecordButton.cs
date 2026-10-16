using System;
using UnityEngine;
using recordPlayer;

public enum ButtonType { StartStop, Pitch33, Pitch45 }

public class RecordButton : MonoBehaviour
{
    public ButtonType buttonType;
    public RecordPlayerController controller;
    public GameObject leftLight;
    public GameObject rightLight;

    public void Start()
    {
        leftLight.SetActive(false);
        rightLight.SetActive(true);
    }

    public void OnPress()
    {
        switch (buttonType)
        {
            case ButtonType.StartStop: controller.TogglePlay();   break;
            case ButtonType.Pitch33:  controller.SetPitch(33); leftLight.SetActive(true); rightLight.SetActive(false);  break;
            case ButtonType.Pitch45:  controller.SetPitch(45); rightLight.SetActive(true); leftLight.SetActive(false);  break;
        }
    }
}