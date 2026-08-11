using UnityEngine;

/// <summary>
/// ScriptableObject that stores the data for a single photo item on a Fotowand.
/// Create a new asset via: right-click in Project > Create > Zensor > Fotowand Data
/// </summary>
[CreateAssetMenu(fileName = "NewFotowandData", menuName = "Zensor/FotowandData")]
public class FotowandData : ScriptableObject
{
    [Header("Photo")]
    [Tooltip("The photo/image displayed when the player opens this Fotowand item")]
    public Sprite photo;

    [Header("Information")]
    [Tooltip("Title of the photo or subject name(e.g. 'Zensor Origins' or 'The Beatles')")]
    public string title;

    [Tooltip("Short description or context shown alongside the photo")]
    [TextArea(3, 8)]
    public string description;

    [Header("Extra Content (Optional)")]
    [Tooltip("Enable if this photo has additional historical or contextual information")]
    public bool hasExtraInfo = false;

    [Tooltip("Heading for the extra info section (e.g. 'About This Band')")]
    public string extraInfoTitle;

    [Tooltip("Long-form text for the extra info - band history, Zensor context, etc.")]
    [TextArea(5, 15)]
    public string extraInfoContent;

    [Tooltip("Optional extra image for the info section(poster, document scan, etc.)")]
    public Sprite extraInfoImage;
}
