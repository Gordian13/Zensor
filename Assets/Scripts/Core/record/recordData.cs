using UnityEngine;


public enum RecordFormat
{
    Vinyl,
    Tape
}

public enum Speed
{
    Fast,
    Slow,
    Normal // for tape
}

[CreateAssetMenu(fileName = "recordData", menuName = "Zensor/recordData")]
public class RecordData : ScriptableObject
{
    public string title;
    public string author;
    public string album;
    public string year;
    public string description; // short summary of the band
    public Sprite sprite; // album cover in jpeg converted to sprite
    public AudioClip audioClip;
    public RecordFormat format;
    public Speed speed; // 33 or 45, using pitch in unity
}
