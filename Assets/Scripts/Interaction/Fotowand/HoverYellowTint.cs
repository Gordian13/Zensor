using UnityEngine;
using Interaction.util.ColorReveal;

public class HoverYellowTint : MonoBehaviour, IColorRevealable
{
    [SerializeField] private Color tintColor = new Color(1f, 0.92f, 0.5f, 1f);
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

    public void SetColorReveal(bool revealed)
    {
        SetColor(revealed);
    }

    public void SetColor(bool showColor)
    {
        if (stayColored && !showColor)
            return;

        SetTint(showColor);
    }

    public void SetStayColored(bool stayColored)
    {
        this.stayColored = stayColored;
        SetColor(stayColored);
    }
}