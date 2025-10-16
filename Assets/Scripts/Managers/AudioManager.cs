using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{   
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [Tooltip("Plays background music continuously")]
    public AudioSource bgmSource;

    [Tooltip("Plays one-shot sound effects")]
    public AudioSource sfxSource;

    [Header("Volume Settings")]
    [Range(0f, 1f)]
    public float bgmVolume = 0.7f;

    [Range(0f, 1f)]
    public float sfxVolume = 1f;

    [Header("Audio Clips")]
    [Tooltip("Drag your AudioClips here to register them")]
    public List<AudioClipData> audioClips = new List<AudioClipData>();

    private Dictionary<string, AudioClip> clipDictionary = new Dictionary<string, AudioClip>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeAudio();
    }

    void InitializeAudio()
    {
        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true; 
            bgmSource.playOnAwake = false;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false; 
            sfxSource.playOnAwake = false;
        }

        bgmSource.volume = bgmVolume;
        sfxSource.volume = sfxVolume;

        foreach (AudioClipData clipData in audioClips)
        {
            if (clipData.clip != null && !clipDictionary.ContainsKey(clipData.clipName))
            {
                clipDictionary.Add(clipData.clipName, clipData.clip);
            }
        }

        Debug.Log($"AudioManager initialized with {clipDictionary.Count} audio clips");
    }

    public void PlayBGM(string clipName)
    {
        if (clipDictionary.TryGetValue(clipName, out AudioClip clip))
        {
            bgmSource.Stop();
            bgmSource.clip = clip;
            bgmSource.Play();
            Debug.Log($"Playing BGM: {clipName}");
        }
        else
        {
            Debug.LogWarning($"BGM not found: {clipName}");
        }
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    public void PauseBGM()
    {
        bgmSource.Pause();
    }

    public void ResumeBGM()
    {
        bgmSource.UnPause();
    }

    public void PlaySFX(string clipName)
    {
        if (clipDictionary.TryGetValue(clipName, out AudioClip clip))
        {
            sfxSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning($"SFX not found: {clipName}");
        }
    }

    public void PlaySFXAtPosition(string clipName, Vector3 position)
    {
        if (clipDictionary.TryGetValue(clipName, out AudioClip clip))
        {
            AudioSource.PlayClipAtPoint(clip, position, sfxVolume);
        }
        else
        {
            Debug.LogWarning($"SFX not found: {clipName}");
        }
    }

    public void PlaySFX(string clipName, float volume)
    {
        if (clipDictionary.TryGetValue(clipName, out AudioClip clip))
        {
            sfxSource.PlayOneShot(clip, volume);
        }
        else
        {
            Debug.LogWarning($"SFX not found: {clipName}");
        }
    }

    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        bgmSource.volume = bgmVolume;
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        sfxSource.volume = sfxVolume;
    }

    public void FadeOutBGM(float duration)
    {
        StartCoroutine(FadeOutCoroutine(duration));
    }

    public void FadeInBGM(string clipName, float duration)
    {
        StartCoroutine(FadeInCoroutine(clipName, duration));
    }

    private System.Collections.IEnumerator FadeOutCoroutine(float duration)
    {
        float startVolume = bgmSource.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }

        bgmSource.Stop();
        bgmSource.volume = startVolume;
    }

    private System.Collections.IEnumerator FadeInCoroutine(string clipName, float duration)
    {
        if (clipDictionary.TryGetValue(clipName, out AudioClip clip))
        {
            bgmSource.Stop();
            bgmSource.clip = clip;
            bgmSource.volume = 0f;
            bgmSource.Play();

            float targetVolume = bgmVolume;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                bgmSource.volume = Mathf.Lerp(0f, targetVolume, elapsed / duration);
                yield return null;
            }

            bgmSource.volume = targetVolume;
        }
    }

    public bool HasClip(string clipName)
    {
        return clipDictionary.ContainsKey(clipName);
    }

    public void PlayRandomSFX(string[] clipNames)
    {
        if (clipNames.Length > 0)
        {
            string randomClip = clipNames[Random.Range(0, clipNames.Length)];
            PlaySFX(randomClip);
        }
    }
}

[System.Serializable]
public class AudioClipData
{
    [Tooltip("Name to reference this clip (e.g., 'PlayerJump', 'EnemyHit')")]
    public string clipName;

    [Tooltip("The actual audio file")]
    public AudioClip clip;
}