using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Handles pointer movement and clicks on a dialogue choice.
/// This connects the TextMeshPro text with Unity's event system.
/// </summary>
public class DialogueChoiceClickHandler : MonoBehaviour, IPointerEnterHandler,
    IPointerExitHandler, IPointerClickHandler
{
    /// <summary>Text element whose color is changed by pointer events.</summary>
    private TMP_Text text;

    /// <summary>Color restored when the pointer leaves the choice.</summary>
    private Color normalColor;

    /// <summary>Color shown while the pointer is over the choice.</summary>
    private Color hoverColor;

    /// <summary>Dialogue action invoked when the choice is clicked.</summary>
    private Action onClick;

    /// <summary>Sets up the text, colors and click action.</summary>
    /// <param name="text">The text component of the choice.</param>
    /// <param name="normalColor">The color used normally.</param>
    /// <param name="hoverColor">The color used while the pointer is over the choice.</param>
    /// <param name="onClick">The action that runs after a click.</param>
    public void Setup(TMP_Text text, Color normalColor, Color hoverColor, Action onClick)
    {
        this.text = text;
        this.normalColor = normalColor;
        this.hoverColor = hoverColor;
        this.onClick = onClick;

        this.text.color = normalColor;
    }

    /// <summary>Applies the hover color.</summary>
    /// <param name="eventData">Data supplied by Unity's event system.</param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (text != null)
            text.color = hoverColor;
    }

    /// <summary>Restores the normal color.</summary>
    /// <param name="eventData">Data supplied by Unity's event system.</param>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (text != null)
            text.color = normalColor;
    }

    /// <summary>Runs the stored click action.</summary>
    /// <param name="eventData">Data supplied by Unity's event system.</param>
    public void OnPointerClick(PointerEventData eventData)
    {
        onClick?.Invoke();
    }
}
