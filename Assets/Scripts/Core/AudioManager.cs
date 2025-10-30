using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Game.Core
{
    [DefaultExecutionOrder(-90)]
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Mixer")]
        public AudioMixer masterMixer;
        public string musicVolumeParam = "MusicVolume";
        public string sfxVolumeParam = "SFXVolume";

        [Header("Sources")]
        public AudioSource musicSource;
        public AudioSource sfxSourcePrefab;

        private readonly List<AudioSource> sfxPool = new();
        private int poolSize = 8;

        [Header("Music")]
        public AudioClip currentMusic;
        private Coroutine musicFadeCoroutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializePool();
        }

        private void InitializePool()
        {
            for (int i = 0; i < poolSize; i++)
            {
                AudioSource src = Instantiate(sfxSourcePrefab, transform);
                src.gameObject.name = $"SFX_{i}";
                src.playOnAwake = false;
                sfxPool.Add(src);
            }
        }

        #region Music

        public void PlayMusic(AudioClip clip, float fadeDuration = 1f)
        {
            if (clip == null) return;
            if (musicSource.clip == clip) return;

            if (musicFadeCoroutine != null)
                StopCoroutine(musicFadeCoroutine);

            musicFadeCoroutine = StartCoroutine(FadeMusic(clip, fadeDuration));
        }

        private System.Collections.IEnumerator FadeMusic(AudioClip newClip, float fadeDuration)
        {
            float startVolume = musicSource.volume;

            // Fade out current track
            for (float t = 0; t < fadeDuration; t += Time.deltaTime)
            {
                musicSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeDuration);
                yield return null;
            }

            musicSource.clip = newClip;
            musicSource.Play();

            // Fade in new track
            for (float t = 0; t < fadeDuration; t += Time.deltaTime)
            {
                musicSource.volume = Mathf.Lerp(0f, startVolume, t / fadeDuration);
                yield return null;
            }

            musicSource.volume = startVolume;
            currentMusic = newClip;
        }

        public void StopMusic()
        {
            if (musicSource.isPlaying)
                musicSource.Stop();
        }

        #endregion

        #region SFX

        public void PlaySFX(string clipName)
        {
            AudioClip clip = Resources.Load<AudioClip>($"Audio/SFX/{clipName}");
            if (clip != null)
                PlaySFX(clip);
            else
                Debug.LogWarning($"SFX '{clipName}' not found in Resources/Audio/SFX/");
        }

        public void PlaySFX(AudioClip clip)
        {
            if (clip == null) return;
            AudioSource source = GetAvailableSFXSource();
            source.clip = clip;
            source.Play();
        }

        private AudioSource GetAvailableSFXSource()
        {
            foreach (var src in sfxPool)
            {
                if (!src.isPlaying)
                    return src;
            }

            // Expand pool if all busy
            AudioSource extra = Instantiate(sfxSourcePrefab, transform);
            extra.playOnAwake = false;
            sfxPool.Add(extra);
            return extra;
        }

        #endregion

        #region Volume

        public void SetMusicVolume(float value)
        {
            masterMixer?.SetFloat(musicVolumeParam, Mathf.Log10(Mathf.Clamp(value, 0.001f, 1f)) * 20f);
        }

        public void SetSFXVolume(float value)
        {
            masterMixer?.SetFloat(sfxVolumeParam, Mathf.Log10(Mathf.Clamp(value, 0.001f, 1f)) * 20f);
        }

        #endregion
    }
}
