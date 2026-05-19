using UnityEngine;

// Applies a visual highlight effect to an NPC when the hover detector tells it to.
// This script does not detect the mouse itself; NPCHoverDetector handles that.
// This separation keeps input detection and visual highlighting independent.
public class NPCHoverHighlight : MonoBehaviour
{
    [Header("References")]
    // NPCController gives access to the NPCProfile, including the configured hover color.
    [SerializeField] private NPCController npcController;

    // Renderers that should change color when the NPC is highlighted.
    // Usually this includes the visible model or placeholder capsule renderer.
    [SerializeField] private Renderer[] renderers;

    [Header("Settings")]
    // Allows the highlight behavior to be disabled from the Inspector without removing the script.
    [SerializeField] private bool highlightEnabled = true;

    // Stores the original colors so they can be restored when the mouse leaves.
    private Color[] originalColors;

    // Tracks whether the NPC is currently highlighted.
    // Prevents repeatedly applying the same state every frame.
    private bool isHighlighted;

    private void Reset()
    {
        // Auto-fill references when the component is added or reset in the Inspector.
        npcController = GetComponentInParent<NPCController>();
        renderers = GetComponentsInChildren<Renderer>();
    }

    private void Awake()
    {
        // Runtime safety: fill missing references if they were not assigned manually.
        if (npcController == null)
            npcController = GetComponentInParent<NPCController>();

        if (renderers == null || renderers.Length == 0)
            renderers = GetComponentsInChildren<Renderer>();

        // Store the original renderer colors before any highlight is applied.
        CacheOriginalColors();
    }

    // Saves the initial material colors.
    // These are restored when highlighting is turned off.
    private void CacheOriginalColors()
    {
        originalColors = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
            originalColors[i] = renderers[i].material.color;
    }

    // Enables or disables the highlight effect.
    // Called by NPCHoverDetector.
    public void SetHighlight(bool active)
    {
        // If highlighting is disabled or profile data is missing, do nothing.
        if (!highlightEnabled || npcController == null || npcController.Profile == null)
            return;

        // Avoid re-applying the same state unnecessarily.
        if (isHighlighted == active)
            return;

        isHighlighted = active;

        // Hover color is configured per NPC profile.
        Color hoverColor = npcController.Profile.hoverColor;

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null)
                continue;

            // If active, blend original color toward hover color.
            // hoverColor.a controls how strong the highlight appears.
            // If inactive, restore the original color.
            renderers[i].material.color = active
                ? Color.Lerp(originalColors[i], hoverColor, hoverColor.a)
                : originalColors[i];
        }
    }
}
