using UnityEngine;
using Interaction.util.ColorReveal;

/// <summary>
/// Tints a Fotowand poster slightly yellow to show that it can be clicked.
/// </summary>
/// <remarks>
/// Uses a <see cref="MaterialPropertyBlock"/> on <c>_BaseColor</c>, so the shared
/// material is not modified. Also implements <see cref="IColorRevealable"/> so the
/// posters take part in the team's spot-based color reveal system.
/// </remarks>
public class HoverYellowTint : MonoBehaviour, IColorRevealable
{
    /// <summary>Tint color applied on hover.</summary>
    [SerializeField] private Color tintColor = new Color(1f, 0.92f, 0.5f, 1f);

    /// <summary>How strongly the tint is blended with white (0 = none, 1 = full tint color).</summary>
    [Range(0f, 1f)]
    [SerializeField] private float tintStrength = 0.2f;

    private Renderer[] renderers;
    private MaterialPropertyBlock propertyBlock;
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    private bool stayColored = false;

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        propertyBlock = new MaterialPropertyBlock();
        SetTint(false);
    }

    /// <summary>
    /// Sets the tint on all child renderers.
    /// </summary>
    /// <param name="isHovered">True to apply the yellow tint, false to reset to white.</param>
    public void SetTint(bool isHovered)
    {
        if (renderers == null || propertyBlock == null)
        {
            renderers = GetComponentsInChildren<Renderer>();
            propertyBlock = new MaterialPropertyBlock();
        }

        Color targetColor = isHovered
            ? Color.Lerp(Color.white, tintColor, tintStrength)
            : Color.white;

        foreach (Renderer r in renderers)
        {
            r.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(BaseColorId, targetColor);
            r.SetPropertyBlock(propertyBlock);
        }
    }

    // --- IColorRevealable ---

    /// <summary>Called by the color reveal system when the wall's spot is revealed or hidden.</summary>
    /// <param name="revealed">True if the spot is revealed.</param>
    public void SetColorReveal(bool revealed)
    {
        SetColor(revealed);
    }

    /// <summary>Applies or removes the tint, unless the poster is locked to stay colored.</summary>
    /// <param name="showColor">True to tint.</param>
    public void SetColor(bool showColor)
    {
        if (stayColored && !showColor)
            return;

        SetTint(showColor);
    }

    /// <summary>Locks the tint on (or unlocks it) so hover-out does not reset it.</summary>
    /// <param name="stayColored">True to keep the tint permanently.</param>
    public void SetStayColored(bool stayColored)
    {
        this.stayColored = stayColored;
        SetColor(stayColored);
    }
}