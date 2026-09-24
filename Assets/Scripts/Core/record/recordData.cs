using System.Collections.Generic;
using UnityEngine;

public enum Speed
{
    Normal, // Standard-Wiedergabe (Pitch 1.0) - auch fuer Tape
    Slow,
    Fast
}

public enum RecordFormat
{
    Vinyl,
    Tape
}

public enum VinylType
{
    SevenInch,
    TwelveInch
}

/**
 * @brief Stores a record's metadata, playback content and visual assets.
 *
 * Create assets through Create > Zensor > recordData. RecordInfoUI reads title,
 * author, album, year and description from the selected record's data. Edit these
 * fields on the asset to update panel contents without changing the UI scripts.
 * Existing record assets are stored in Assets/Scripts/Core/record/VinylSongs/.
 */
[CreateAssetMenu(fileName = "recordData", menuName = "Zensor/recordData")]
public class RecordData : ScriptableObject
{
    [Header("Metadata")]
    public string title;
    public string author;
    public string album;
    public string year;
    /**
     * @brief Background text displayed in the record information panel.
     * The Inspector provides a multiline editing area; empty or whitespace-only
     * values cause RecordInfoUI to display its configured fallback description.
     */
    [TextArea(4, 12)]
    public string description;

    [Header("Visuals")]
    public Sprite sprite;

    [Header("Format")]
    public RecordFormat format = RecordFormat.Vinyl;
    public Speed speed;

    [Header("Vinyl Type")]
    public VinylType vinylType = VinylType.TwelveInch;

    [Header("Tracks")]
    public List<AudioClip> tracks = new List<AudioClip>();

    // Convenience: return the first track for backwards compatibility.
    public AudioClip audioClip => TrackCount > 0 ? tracks[0] : null;

    public AudioClip GetTrack(int index)
    {
        if (tracks == null || index < 0 || index >= tracks.Count) return null;
        return tracks[index];
    }

    public int TrackCount => tracks?.Count ?? 0;

    [Header("Textures")]
    public Texture2D labelFrontTexture;
    public Texture2D labelBackTexture;
    public Texture2D coverFrontTexture;
    public Texture2D coverBackTexture;
}
