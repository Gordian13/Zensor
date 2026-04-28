using UnityEngine;

[CreateAssetMenu(fileName = "recordData", menuName = "Zensor/recordData")]
public class RecordData : ScriptableObject
{
    public string title;
    public string author;
    public string description;
    public Sprite sprite;
    public AudioClip audioClip;
}
