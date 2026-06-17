using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class DialogueChoiceClickHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private TMP_Text text;
    private Color normalColor;
    private Color hoverColor;
    private Action onClick;

    public void Setup(TMP_Text text, Color normalColor, Color hoverColor, Action onClick)
    {
        this.text = text;
        this.normalColor = normalColor;
        this.hoverColor = hoverColor;
        this.onClick = onClick;

        this.text.color = normalColor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (text != null)
            text.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (text != null)
            text.color = normalColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        onClick?.Invoke();
    }
}