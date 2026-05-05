using UnityEngine;


public enum RecordFormat
{
    Vinyl,
    Cassette
}

public enum Speed
{
    Fast,
    Slow
}

[CreateAssetMenu(fileName = "recordData", menuName = "Zensor/recordData")]
public class RecordData : ScriptableObject
{
    public string title;
    public string author;
    public string description;
    public Sprite sprite; // album cover in jpeg
    public AudioClip audioClip;
    public RecordFormat format;
    public Speed speed; // 33 or 45, using pitch in unity
}
