using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.IO;

public class SettingsManager : MonoBehaviour
{
    [Header("UI Components")]
    public Slider musicVolumeSlider;
    public Button musicMuteButton;
    public TextMeshProUGUI musicMuteText;

    public Slider sfxVolumeSlider;
    public Button sfxMuteButton;
    public TextMeshProUGUI sfxMuteText;

    public TMP_Dropdown resolutionDropdown;
    public Button backButton;
    public Toggle fullscreenToggle;

    [Header("Resolutions (16:9)")]
    private Resolution[] resolutions = {
        new Resolution { width = 1280, height = 720 },
        new Resolution { width = 1366, height = 768 },
        new Resolution { width = 1600, height = 900 },
        new Resolution { width = 1920, height = 1080 }
    };

    private void Start()
    {
        InitializeUI();
    }

    private void InitializeUI()
    {
        SettingsData settings = LoadSettings();

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = settings.musicVolume;
            musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        }

        if (musicMuteButton != null)
        {
            musicMuteButton.onClick.AddListener(ToggleMusicMute);
            UpdateMusicMuteButtonText(settings.isMusicMuted);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = settings.sfxVolume;
            sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
        }

        if (sfxMuteButton != null)
        {
            sfxMuteButton.onClick.AddListener(ToggleSFXMute);
            UpdateSFXMuteButtonText(settings.isSfxMuted);
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(settings.musicVolume);
            AudioManager.Instance.ToggleMusicMute(settings.isMusicMuted);
            AudioManager.Instance.SetSFXVolume(settings.sfxVolume);
            AudioManager.Instance.ToggleSFXMute(settings.isSfxMuted);
        }
        else
        {
            Debug.LogError("AudioManager instance is not set.");
        }

        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = settings.isFullscreen;
            fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
            Screen.fullScreen = settings.isFullscreen;
        }

        if (resolutionDropdown != null)
        {
            resolutionDropdown.ClearOptions();
            foreach (var res in resolutions)
            {
                resolutionDropdown.options.Add(new TMP_Dropdown.OptionData($"{res.width} x {res.height}"));
            }
            resolutionDropdown.value = settings.resolutionIndex;
            resolutionDropdown.onValueChanged.AddListener(SetResolution);
            SetResolution(settings.resolutionIndex);
        }

        if (backButton != null)
        {
            backButton.onClick.AddListener(GoBack);
        }
    }

    private void UpdateMusicMuteButtonText(bool isMuted)
    {
        if (musicMuteText != null)
        {
            musicMuteText.text = isMuted ? "Unmute" : "Mute";
        }
    }

    private void UpdateSFXMuteButtonText(bool isMuted)
    {
        if (sfxMuteText != null)
        {
            sfxMuteText.text = isMuted ? "Unmute" : "Mute";
        }
    }

    public void SetMusicVolume(float volume)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(volume);
            SaveSettings();
        }
    }

    public void ToggleMusicMute()
    {
        if (AudioManager.Instance != null)
        {
            bool newMuteState = !AudioManager.Instance.IsMusicMuted();
            AudioManager.Instance.ToggleMusicMute(newMuteState);
            UpdateMusicMuteButtonText(newMuteState);
            SaveSettings();
        }
    }

    public void SetSFXVolume(float volume)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(volume);
            SaveSettings();
        }
    }

    public void ToggleSFXMute()
    {
        if (AudioManager.Instance != null)
        {
            bool newMuteState = !AudioManager.Instance.IsSFXMuted();
            AudioManager.Instance.ToggleSFXMute(newMuteState);
            UpdateSFXMuteButtonText(newMuteState);
            SaveSettings();
        }
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        SaveSettings();
    }

    public void SetResolution(int index)
    {
        Resolution res = resolutions[index];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
        SaveSettings();
    }

    public void GoBack()
    {
        SceneManager.LoadScene("MainMenu");
    }

    private void SaveSettings()
    {
        SettingsData settings = new SettingsData();

        if (AudioManager.Instance != null)
        {
            settings.musicVolume = AudioManager.Instance.GetMusicVolume();
            settings.isMusicMuted = AudioManager.Instance.IsMusicMuted();
            settings.sfxVolume = AudioManager.Instance.GetSFXVolume();
            settings.isSfxMuted = AudioManager.Instance.IsSFXMuted();
        }
        else
        {
            settings.musicVolume = musicVolumeSlider?.value ?? 1f;
            settings.isMusicMuted = false;
            settings.sfxVolume = sfxVolumeSlider?.value ?? 1f;
            settings.isSfxMuted = false;
        }

        settings.resolutionIndex = resolutionDropdown?.value ?? 3;
        settings.isFullscreen = Screen.fullScreen;

        SaveManager.SaveData(settings);
    }

    public static SettingsData LoadSettings()
    {
        string path = Application.persistentDataPath + "/settings.json";
        if (File.Exists(path))
        {
            return JsonUtility.FromJson<SettingsData>(File.ReadAllText(path));
        }
        return new SettingsData
        {
            musicVolume = 1f,
            isMusicMuted = false,
            sfxVolume = 1f,
            isSfxMuted = false,
            resolutionIndex = 3,
            isFullscreen = true
        };
    }
}

[System.Serializable]
public class SettingsData
{
    public float musicVolume = 0.5f;
    public bool isMusicMuted = false;
    public float sfxVolume = 0.8f;
    public bool isSfxMuted = false;
    public int resolutionIndex = 3;
    public bool isFullscreen = true;
}
