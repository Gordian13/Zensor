using UnityEngine;
using Core.camera;
using Interaction.util.ColorReveal;

/// <summary>
/// Attach this to decorative (non-interactable) posters that should stay
/// black-and-white whenever the player is standing at this poster's spot,
/// even if another script (e.g. ColorRevealHoverCamera) tries to hover-color them.
/// </summary>
/// <remarks>
/// Requires a <see cref="ColorRevealToggle"/> on the same GameObject.
/// Runs in <c>LateUpdate</c> so it is applied after ColorRevealHoverCamera
/// (which runs in <c>Update</c>) and therefore always wins.
/// </remarks>
public class ForceGrayscaleAtSpot : MonoBehaviour
{
    /// <summary>Camera spot of the wall this poster belongs to. Defaults to the parent <see cref="CameraSpot"/>.</summary>
    [SerializeField] private CameraSpot owningSpot;
    /// <summary>Used to check the player's current spot. Found automatically if empty.</summary>
    [SerializeField] private SpotManager spotManager;

    private ColorRevealToggle colorRevealToggle;

    private void Awake()
    {
        colorRevealToggle = GetComponent<ColorRevealToggle>();

        if (owningSpot == null)
            owningSpot = GetComponentInParent<CameraSpot>();

        if (spotManager == null)
            spotManager = FindFirstObjectByType<SpotManager>();
    }

    private void LateUpdate()
    {
        if (colorRevealToggle == null || spotManager == null || owningSpot == null)
            return;

        if (spotManager.IsCurrentSpot(owningSpot))
        {
            // The player is standing in front of this wall -> force back to B&W,
            // overriding ColorRevealHoverCamera, which runs in Update().
            colorRevealToggle.SetColor(false);
        }
    }
}