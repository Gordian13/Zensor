using System.Collections.Generic;
using UnityEngine;

namespace background
{
    /**
     * @brief Plays random background music on all registered MusicBoxes.
     *
     * Music Boxes register here using the Observer pattern.
     */
    public class BackGroundMusicManager : MonoBehaviour
    {
        /** The single instance of the manager (Singleton Pattern). */
        public static BackGroundMusicManager Instance { get; private set; }

        /** Songs that are picked at random. */
        [Header("Songs")]
        public AudioClip[] songs;
        /** AudioSources of all registered MusicBoxes. */
        private List<AudioSource> targetedBoxes = new();
     
        [Header("Playback")]
        public float fadeSpeed = 2f;
        public float volume = 0.1f;

        /** The song that is currently playing. */
        AudioClip _currentClip;
        /** True while the music is fading out or paused. */
        bool stopping;

        /** Sets up the Singleton and keeps the manager alive across scene loads. */
        void Awake()
        {
            PlayBackGroundMusic();
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /** Fades out and pauses the music on all music boxes. */
        public void StopBackGroundMusic() => stopping = true;

        /** Resumes the music on all music boxes. */
        public void PlayBackGroundMusic()
        {
            stopping = false;
            foreach (var s in targetedBoxes)
            {
                s.volume = volume;
                if (!s.isPlaying) s.UnPause();
            }
        }

        /** Fades out the music while stopping, otherwise starts a new song when the current one has ended. */
        void Update()
        {
            //Debug.Log(stopping);
            //Debug.Log(targetedBoxes.Count);
            if (targetedBoxes.Count == 0) return; 

            if (stopping)
            {
                foreach (var s in targetedBoxes)
                {
                    s.volume = Mathf.MoveTowards(s.volume, 0f, fadeSpeed * Time.deltaTime);
                    if (s.volume <= 0f) s.Pause();
                }
                return;
            }
            
            if (!targetedBoxes[0].isPlaying)
                PlayRandomOnAll();
        }

        /** Picks a random song that differs from the current one and plays it on all music boxes. */
        void PlayRandomOnAll()
        {
            if (songs.Length == 0) return;

            AudioClip newSong = _currentClip;
            while (songs.Length > 1 && newSong == _currentClip)
                newSong = songs[Random.Range(0, songs.Length)];

            if (newSong == null) newSong = songs[0];
            _currentClip = newSong;

            foreach (var s in targetedBoxes)
            {
                s.volume = volume;
                s.clip = newSong;
                s.Play();
            }
        }

        /**
         * Adds a music box to the manager.
         *
         * @param src AudioSource of the music box.
         */
        public void Register(AudioSource src)
        {
            if (!targetedBoxes.Contains(src)) targetedBoxes.Add(src);
        }

        /**
         * Removes a music box from the manager.
         *
         * @param src AudioSource of the music box.
         */
        public void Unregister(AudioSource src) => targetedBoxes.Remove(src);
    }
}