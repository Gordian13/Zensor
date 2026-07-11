using UnityEngine;

/// <summary>
/// Attach this to posters whose base is static black-and-white.
/// Tints the whole surface slightly yellow while the poster is being hovered.
/// Works alongside FotowandInteractable via SetHighlight().
/// </summary>
public class HoverYellowTint : MonoBehaviour
{
    [SerializeField] private Color tintColor = new Color(1f, 0.92f, 0.5f, 1f);
    [Range(0f, 1f)]
    [SerializeField] private float tintStrength = 0.2f;

    private Renderer[] renderers;
    private MaterialPropertyBlock propertyBlock;
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        propertyBlock = new MaterialPropertyBlock();
        SetTint(false);
    }

    public void SetTint(bool isHovered)
    {
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
}