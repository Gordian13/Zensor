using UnityEngine;

/// <summary>
/// Contract for any object on the photo wall (Fotowand) that the player can
/// hover and click to open the info panel.
/// </summary>
/// <remarks>
/// Implemented by <see cref="FotowandInteractable"/>. <see cref="Core.FotowandSelect.FotowandBoxSelector"/>
/// finds implementers via raycast, so the selector does not need to know the concrete class.
/// </remarks>
public interface IFotowand
{
    /// <summary>Returns the content (photo, title, texts) to show in the info panel.</summary>
    /// <returns>The <see cref="FotowandData"/> asset assigned to this item.</returns>
    FotowandData GetData();

    /// <summary>Returns the transform that represents this item in the scene.</summary>
    /// <returns>The item's root transform.</returns>
    Transform GetSelectionTransform();

    /// <summary>Turns the hover highlight on or off.</summary>
    /// <param name="isHighlighted">True while the cursor is over the item.</param>
    void SetHighlight(bool isHighlighted);
}
