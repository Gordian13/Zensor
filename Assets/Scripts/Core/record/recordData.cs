using UnityEngine;
using System.Collections.Generic;



public enum Speed
{
    Fast,
    Slow,
    Normal // for tape
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
    public Speed speed;

    [Header("Tracks")]
    public List<AudioClip> tracks = new List<AudioClip>();

    // Convenience: ersten Track zurückgeben (Abwärtskompatibilität)
    public AudioClip audioClip => tracks.Count > 0 ? tracks[0] : null;

    public AudioClip GetTrack(int index)
    {
        if (index < 0 || index >= tracks.Count) return null;
        return tracks[index];
    }

    public int TrackCount => tracks.Count;
}
