using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Background Music Settings")]
    public AudioSource backgroundMusic;
    [Range(0f, 1f)]
    public float musicVolume = 0.5f;
    private bool isMusicMuted = false;

    [Header("SFX Settings")]
    [Range(0f, 1f)]
    public float sfxVolume = 0.8f;
    [Range(0.5f, 1.5f)]
    public float minSfxPitch = 0.95f;
    [Range(0.5f, 1.5f)]
    public float maxSfxPitch = 1.05f;
    private bool isSfxMuted = false;

    [Header("Music Clips")]
    public AudioClip mainMenuMusic;
    public AudioClip gameplayMusic;
    public AudioClip resultMusic;

    [Header("Dialog Audio")]
    public AudioClip dialogueLetterSFX;
    public AudioClip popSFX;
    public AudioClip buttonClickSFX;

    [Header("Audio Pool Settings")]
    public int initialPoolSize = 10;
    public int maxPoolSize = 30;
    [Tooltip("Automatically destroy unused sources after this time")]
    public float unusedSourceTimeout = 10f;

    private List<PooledAudioSource> audioSourcePool = new List<PooledAudioSource>();
    private Coroutine currentFade;
    private Transform poolParent;

    private class PooledAudioSource
    {
        public AudioSource source;
        public bool isPlaying;
        public float lastUsedTime;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        poolParent = new GameObject("AudioSourcePool").transform;
        poolParent.SetParent(transform);

        InitializeAudioPool();
        LoadSettings();
    }

    private void InitializeAudioPool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreatePooledAudioSource();
        }
    }

    private PooledAudioSource CreatePooledAudioSource()
    {
        GameObject sourceObj = new GameObject("PooledAudioSource");
        sourceObj.transform.SetParent(poolParent);

        AudioSource source = sourceObj.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;

        PooledAudioSource pooledSource = new PooledAudioSource
        {
            source = source,
            isPlaying = false,
            lastUsedTime = Time.time
        };

        audioSourcePool.Add(pooledSource);
        return pooledSource;
    }

    private void Update()
    {
        for (int i = 0; i < audioSourcePool.Count; i++)
        {
            if (audioSourcePool[i].isPlaying && !audioSourcePool[i].source.isPlaying)
            {
                audioSourcePool[i].isPlaying = false;
                audioSourcePool[i].lastUsedTime = Time.time;
            }
        }

        if (audioSourcePool.Count > initialPoolSize)
        {
            CleanUpUnusedSources();
        }
    }

    private void CleanUpUnusedSources()
    {
        float currentTime = Time.time;
        for (int i = audioSourcePool.Count - 1; i >= initialPoolSize; i--)
        {
            var pooledSource = audioSourcePool[i];
            if (!pooledSource.isPlaying && (currentTime - pooledSource.lastUsedTime) > unusedSourceTimeout)
            {
                Destroy(pooledSource.source.gameObject);
                audioSourcePool.RemoveAt(i);
            }
        }
    }

    private PooledAudioSource GetAvailableAudioSource()
    {
        for (int i = 0; i < audioSourcePool.Count; i++)
        {
            if (!audioSourcePool[i].isPlaying)
            {
                audioSourcePool[i].lastUsedTime = Time.time;
                return audioSourcePool[i];
            }
        }

        if (audioSourcePool.Count < maxPoolSize)
        {
            return CreatePooledAudioSource();
        }

        PooledAudioSource oldestSource = audioSourcePool[0];
        float oldestTime = float.MaxValue;

        for (int i = 0; i < audioSourcePool.Count; i++)
        {
            if (audioSourcePool[i].lastUsedTime < oldestTime)
            {
                oldestTime = audioSourcePool[i].lastUsedTime;
                oldestSource = audioSourcePool[i];
            }
        }

        oldestSource.source.Stop();
        oldestSource.lastUsedTime = Time.time;
        return oldestSource;
    }

    private void Start()
    {
        if (backgroundMusic != null && !backgroundMusic.isPlaying)
        {
            backgroundMusic.volume = musicVolume;
            backgroundMusic.Play();
        }
    }

    private void LoadSettings()
    {
        if (PlayerPrefs.HasKey("MusicVolume"))
            musicVolume = PlayerPrefs.GetFloat("MusicVolume");

        if (PlayerPrefs.HasKey("MusicMuted"))
            isMusicMuted = PlayerPrefs.GetInt("MusicMuted") == 1;

        if (PlayerPrefs.HasKey("SFXVolume"))
            sfxVolume = PlayerPrefs.GetFloat("SFXVolume");

        if (PlayerPrefs.HasKey("SFXMuted"))
            isSfxMuted = PlayerPrefs.GetInt("SFXMuted") == 1;

        if (PlayerPrefs.HasKey("MinSFXPitch"))
            minSfxPitch = PlayerPrefs.GetFloat("MinSFXPitch");

        if (PlayerPrefs.HasKey("MaxSFXPitch"))
            maxSfxPitch = PlayerPrefs.GetFloat("MaxSFXPitch");
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.SetInt("MusicMuted", isMusicMuted ? 1 : 0);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.SetInt("SFXMuted", isSfxMuted ? 1 : 0);
        PlayerPrefs.SetFloat("MinSFXPitch", minSfxPitch);
        PlayerPrefs.SetFloat("MaxSFXPitch", maxSfxPitch);
        PlayerPrefs.Save();
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (backgroundMusic != null && !isMusicMuted)
        {
            backgroundMusic.volume = musicVolume;
        }
        SaveSettings();
    }

    public void ToggleMusicMute(bool mute)
    {
        isMusicMuted = mute;
        if (backgroundMusic != null)
        {
            backgroundMusic.mute = mute;
            if (!mute)
                backgroundMusic.volume = musicVolume;
        }
        SaveSettings();
    }

    public float GetMusicVolume()
    {
        return musicVolume;
    }

    public bool IsMusicMuted()
    {
        return isMusicMuted;
    }

    public void PlayMusic(AudioClip clip, float volume = -1f, float fadeDuration = 1f)
    {
        if (backgroundMusic == null || clip == null)
            return;

        if (backgroundMusic.clip == clip && backgroundMusic.isPlaying)
            return;

        if (currentFade != null)
            StopCoroutine(currentFade);
        currentFade = StartCoroutine(FadeInNewMusic(clip, volume >= 0 ? volume : musicVolume, fadeDuration));
    }

    public void StopMusic(float fadeDuration = 1f)
    {
        if (backgroundMusic == null || !backgroundMusic.isPlaying)
            return;

        if (currentFade != null)
            StopCoroutine(currentFade);
        currentFade = StartCoroutine(FadeOutMusic(fadeDuration));
    }

    private IEnumerator FadeOutMusic(float duration)
    {
        float startVolume = backgroundMusic.volume;
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            backgroundMusic.volume = Mathf.Lerp(startVolume, 0f, t / duration);
            yield return null;
        }
        backgroundMusic.volume = 0f;
        backgroundMusic.Stop();
        backgroundMusic.clip = null;
    }

    private IEnumerator FadeInNewMusic(AudioClip newClip, float targetVolume, float duration)
    {
        if (backgroundMusic.isPlaying)
        {
            yield return StartCoroutine(FadeOutMusic(duration));
        }
        backgroundMusic.clip = newClip;
        backgroundMusic.Play();
        backgroundMusic.volume = 0f;
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            backgroundMusic.volume = Mathf.Lerp(0f, targetVolume, t / duration);
            yield return null;
        }
        backgroundMusic.volume = targetVolume;
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        SaveSettings();
    }

    public void ToggleSFXMute(bool mute)
    {
        isSfxMuted = mute;
        SaveSettings();
    }

    public float GetSFXVolume()
    {
        return sfxVolume;
    }

    public bool IsSFXMuted()
    {
        return isSfxMuted;
    }

    public float GetMinSFXPitch()
    {
        return minSfxPitch;
    }

    public float GetMaxSFXPitch()
    {
        return maxSfxPitch;
    }

    public void SetSFXPitchRange(float min, float max)
    {
        minSfxPitch = Mathf.Clamp(min, 0.5f, 1.5f);
        maxSfxPitch = Mathf.Clamp(max, minSfxPitch, 1.5f);
        SaveSettings();
    }

    // -------------------- Pooled SFX Methods --------------------

    public void PlaySFX(AudioClip clip, float volumeMultiplier = 1f, bool randomPitch = true)
    {
        if (isSfxMuted || clip == null)
            return;

        PooledAudioSource pooledSource = GetAvailableAudioSource();
        if (pooledSource == null)
            return;

        AudioSource source = pooledSource.source;
        source.clip = clip;
        source.volume = sfxVolume * volumeMultiplier;
        source.pitch = randomPitch ? Random.Range(minSfxPitch, maxSfxPitch) : 1f;
        source.Play();

        pooledSource.isPlaying = true;
    }

    // -------------------- Dialog Audio Methods --------------------

    public void PlayDialogLetterSound(float volumeMultiplier = 0.5f)
    {
        PlaySFX(dialogueLetterSFX, volumeMultiplier, true);
    }

    public void PlayButtonClickSound()
    {
        PlaySFX(buttonClickSFX, 1f, false);
    }

    public void PlayPopSound()
    {
        PlaySFX(popSFX, 1f, true);
    }

    public void PlayMainMenuMusic()
    {
        if (mainMenuMusic != null)
        {
            PlayMusic(mainMenuMusic, musicVolume);
        }
    }

    public void PlayGameplayMusic()
    {
        if (gameplayMusic != null)
        {
            PlayMusic(gameplayMusic, musicVolume);
        }
    }

    public void PlayResultMusic()
    {
        if (resultMusic != null)
        {
            PlayMusic(resultMusic, musicVolume);
        }
    }
}