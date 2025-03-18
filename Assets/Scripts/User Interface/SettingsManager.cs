using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class SettingsManager : MonoBehaviour
{
    [Header("UI Components")]
    public Slider volumeSlider;
    public Button muteButton;
    public TMP_Dropdown resolutionDropdown;
    public Button backButton;

    [Header("Audio Settings")]
    private float previousVolume = 1f;
    private bool isMuted = false;

    [Header("Resolutions (16:9)")]
    private Resolution[] resolutions = {
        new Resolution { width = 1280, height = 720 },
        new Resolution { width = 1366, height = 768 },
        new Resolution { width = 1600, height = 900 },
        new Resolution { width = 1920, height = 1080 },
        new Resolution { width = 2560, height = 1440 },
        new Resolution { width = 3840, height = 2160 }
    };

    private string previousScene;

    private void Start()
    {
        volumeSlider.value = PlayerPrefs.GetFloat("Volume", 1f);
        isMuted = PlayerPrefs.GetInt("Muted", 0) == 1;
        previousVolume = volumeSlider.value;

        AudioListener.volume = isMuted ? 0 : volumeSlider.value;

        volumeSlider.onValueChanged.AddListener(SetVolume);
        muteButton.onClick.AddListener(ToggleMute);

        resolutionDropdown.ClearOptions();
        foreach (var res in resolutions)
        {
            resolutionDropdown.options.Add(new TMP_Dropdown.OptionData($"{res.width} x {res.height}"));
        }

        int savedResIndex = PlayerPrefs.GetInt("ResolutionIndex", 3);
        resolutionDropdown.value = savedResIndex;
        resolutionDropdown.onValueChanged.AddListener(SetResolution);
        SetResolution(savedResIndex);
    }

    public void SetVolume(float volume)
    {
        if (!isMuted)
        {
            AudioListener.volume = volume;
            previousVolume = volume;
            PlayerPrefs.SetFloat("Volume", volume);
            PlayerPrefs.Save();
        }
    }

    public void ToggleMute()
    {
        isMuted = !isMuted;
        AudioListener.volume = isMuted ? 0 : previousVolume;
        PlayerPrefs.SetInt("Muted", isMuted ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetResolution(int index)
    {
        Resolution res = resolutions[index];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
        PlayerPrefs.SetInt("ResolutionIndex", index);
        PlayerPrefs.Save();
    }

    public void GoBack()
    {
        SceneManager.LoadScene("MainMenu");
    }

}
