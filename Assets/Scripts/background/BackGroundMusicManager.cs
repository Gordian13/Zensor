using System.Collections.Generic;
using UnityEngine;

namespace background
{
    public class BackGroundMusicManager : MonoBehaviour
    {
        public static BackGroundMusicManager Instance { get; private set; }

        public AudioClip[] songs;
        private List<AudioSource> targetedBoxes = new();
        public float fadeSpeed = 2f;
        public float volume = 0.1f;

        AudioClip _currentClip;
        bool stopping;

        void Awake()
        {
            PlayMusic();
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void StopMusic() => stopping = true;

        public void PlayMusic()
        {
            stopping = false;
            foreach (var s in targetedBoxes) { s.volume = volume; s.Play(); }
        }

        void Update()
        {
            Debug.Log(stopping);
            Debug.Log(targetedBoxes.Count);
            if (targetedBoxes.Count == 0) return; 

            if (stopping)
            {
                foreach (var s in targetedBoxes)
                {
                    s.volume = Mathf.MoveTowards(s.volume, 0f, fadeSpeed * Time.deltaTime);
                    if (s.volume <= 0f) s.Stop();
                }
                return;
            }
            
            if (!targetedBoxes[0].isPlaying)
                PlayRandomOnAll();
        }

        void PlayRandomOnAll()
        {
            AudioClip newSong = _currentClip;
            while (songs.Length > 1 && newSong == _currentClip)
                newSong = songs[Random.Range(0, songs.Length)];

            _currentClip = newSong;

            foreach (var s in targetedBoxes)
            {
                s.volume = volume;
                s.clip = newSong;
                s.Play();
            }
        }

        public void Register(AudioSource src)
        {
            if (!targetedBoxes.Contains(src)) targetedBoxes.Add(src);
        }

        public void Unregister(AudioSource src) => targetedBoxes.Remove(src);
    }
}