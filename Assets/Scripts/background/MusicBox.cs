using background;
using UnityEngine;

/**
 * @brief Music box on the walls that registers itself with the background::BackGroundMusicManager.
 *
 * Follows the Observer pattern: the box subscribes in OnEnable and unsubscribes in OnDisable.
 * The script only needs an AudioSource on the same GameObject.
 * The Logic with Playing etc is all in the background::BackGroundMusicManager
 */
public class MusicBox : MonoBehaviour
{
    AudioSource src;

    /** Takes the AudioSource, that should be on the same Object and puts it in src. */
    void Awake() => src = GetComponent<AudioSource>();

    /** Registers with the background::BackGroundMusicManager::Register. */
    void OnEnable()  => BackGroundMusicManager.Instance?.Register(src);

    /** Removes this from the background::BackGroundMusicManager::Unregister. */
    void OnDisable() => BackGroundMusicManager.Instance?.Unregister(src);
}
