using UnityEngine;

/// <summary>
/// A single clickable photo/poster on the Fotowand.
/// </summary>
/// <remarks>
/// Setup: attach to the poster GameObject together with a Collider (on the layer
/// used by <see cref="Core.FotowandSelect.FotowandBoxSelector"/>) and a
/// <see cref="HoverYellowTint"/>, then assign a <see cref="FotowandData"/> asset.
/// The poster must be a child of the <c>CameraSpot</c> that owns the wall.
/// Clicking is handled by <see cref="Core.FotowandSelect.FotowandBoxSelector"/>, not by this script.
/// </remarks>
public class FotowandInteractable : MonoBehaviour, IFotowand
{
    /// <summary>Content shown in the info panel when this poster is clicked.</summary>
    [Header("Data")]
    public FotowandData data;

    private HoverYellowTint hoverYellowTint;

    /// <inheritdoc/>
    public FotowandData GetData() => data;

    /// <inheritdoc/>
    public Transform GetSelectionTransform() => transform;

    private void Awake()
    {
        hoverYellowTint = GetComponent<HoverYellowTint>();
        SetHighlight(false);
    }

    /// <summary>
    /// Applies or removes the yellow hover tint via <see cref="HoverYellowTint"/>.
    /// </summary>
    /// <param name="isHighlighted">True to tint, false to reset.</param>
    public void SetHighlight(bool isHighlighted)
    {
        if (hoverYellowTint != null)
            hoverYellowTint.SetTint(isHighlighted);
    }
}
