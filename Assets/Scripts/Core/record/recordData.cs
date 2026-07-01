using System.Collections.Generic;
using UnityEngine;

public enum Speed
{
    Fast,
    Slow,
    Normal // for tape
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

[CreateAssetMenu(fileName = "recordData", menuName = "Zensor/recordData")]
public class RecordData : ScriptableObject
{
    [Header("Metadata")]
    public string title;
    public string author;
    public string album;
    public string year;
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