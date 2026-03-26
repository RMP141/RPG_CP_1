using UnityEngine;
using System.Collections.Generic;

namespace RPG.Core
{
    public class AudioManager : IAudioManager
    {
        private AudioSource musicSource;
        private AudioSource sfxSource;
        private Dictionary<string, AudioClip> musicClips = new Dictionary<string, AudioClip>();
        private Dictionary<string, AudioClip> sfxClips = new Dictionary<string, AudioClip>();

        public AudioManager()
        {
            var audioGO = new GameObject("AudioManager");
            Object.DontDestroyOnLoad(audioGO);
            musicSource = audioGO.AddComponent<AudioSource>();
            sfxSource = audioGO.AddComponent<AudioSource>();

            musicSource.loop = true;
            musicSource.volume = 0.5f;
            sfxSource.volume = 0.7f;

            LoadClips();
        }

        private void LoadClips()
        {
            // Загрузка из Resources
            var loadedMusic = Resources.LoadAll<AudioClip>("Audio/Music");
            foreach (var clip in loadedMusic)
            {
                musicClips[clip.name] = clip;
            }

            var loadedSFX = Resources.LoadAll<AudioClip>("Audio/SFX");
            foreach (var clip in loadedSFX)
            {
                sfxClips[clip.name] = clip;
            }
        }

        public void PlayMusic(string name)
        {
            if (musicClips.TryGetValue(name, out var clip))
            {
                musicSource.clip = clip;
                musicSource.Play();
            }
        }

        public void PlaySFX(string name)
        {
            if (sfxClips.TryGetValue(name, out var clip))
            {
                sfxSource.PlayOneShot(clip);
            }
        }

        public void StopMusic() => musicSource.Stop();
        public void SetMusicVolume(float volume) => musicSource.volume = volume;
        public void SetSFXVolume(float volume) => sfxSource.volume = volume;
    }
}