using UnityEngine;
using Core.camera;
using Interaction.util.ColorReveal;

/// <summary>
/// Attach this to decorative (non-interactable) posters that should stay
/// black-and-white whenever the player is standing at this poster's spot,
/// even if another script (e.g. ColorRevealHoverCamera) tries to hover-color them.
/// </summary>
public class ForceGrayscaleAtSpot : MonoBehaviour
{
    [SerializeField] private CameraSpot owningSpot;
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
            // Kita lagi berdiri di depan wall ini -> paksa balik B&W,
            // menang dari ColorRevealHoverCamera yang jalan di Update().
            colorRevealToggle.SetColor(false);
        }
    }
}