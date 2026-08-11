using background;
using UnityEngine;

public class MusicBox : MonoBehaviour
{
    AudioSource src;

    void Awake() => src = GetComponent<AudioSource>();
    void OnEnable()  => BackGroundMusicManager.Instance?.Register(src);
    void OnDisable() => BackGroundMusicManager.Instance?.Unregister(src);
}