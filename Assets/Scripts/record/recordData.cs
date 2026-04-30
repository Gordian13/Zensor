using UnityEngine;

[CreateAssetMenu(fileName = "recordData", menuName = "Zensor/recordData")]
public class RecordData : ScriptableObject
{
    public string title;
    public string author;
    public int year;
    public string description;
    public Sprite sprite;
    public AudioClip audioClip;
    public bool isPlayable = true;
}
