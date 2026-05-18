using UnityEngine;

public class NPCHoverHighlight : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NPCController npcController;
    [SerializeField] private Renderer[] renderers;

    [Header("Settings")]
    [SerializeField] private bool highlightEnabled = true;

    private Color[] originalColors;
    private bool isHighlighted;

    private void Reset()
    {
        npcController = GetComponentInParent<NPCController>();
        renderers = GetComponentsInChildren<Renderer>();
    }

    private void Awake()
    {
        if (npcController == null)
            npcController = GetComponentInParent<NPCController>();

        if (renderers == null || renderers.Length == 0)
            renderers = GetComponentsInChildren<Renderer>();

        CacheOriginalColors();
    }

    private void CacheOriginalColors()
    {
        originalColors = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
            originalColors[i] = renderers[i].material.color;
    }

    public void SetHighlight(bool active)
    {
        if (!highlightEnabled || npcController == null || npcController.Profile == null)
            return;

        if (isHighlighted == active)
            return;

        isHighlighted = active;

        Color hoverColor = npcController.Profile.hoverColor;

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null)
                continue;

            renderers[i].material.color = active
                ? Color.Lerp(originalColors[i], hoverColor, hoverColor.a)
                : originalColors[i];
        }
    }
}