using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.IO;

public class SettingsManager : MonoBehaviour
{
    [Header("UI Components")]
    public Slider volumeSlider;
    public Button muteButton;
    public TMP_Dropdown resolutionDropdown;
    public Button backButton;

    [Header("Audio Settings")]
    private float previousVolume;
    private bool isMuted = false;

    [Header("Resolutions (16:9)")]
    private Resolution[] resolutions = {
            new Resolution { width = 1280, height = 720 },
            new Resolution { width = 1366, height = 768 },
            new Resolution { width = 1600, height = 900 },
            new Resolution { width = 1920, height = 1080 }
        };

    private string previousScene;


    private void Start()
    {
        SettingsData settings = LoadSettings();
        volumeSlider.value = settings.volume;
        isMuted = settings.isMuted;
        previousVolume = volumeSlider.value;

        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.SetVolume(isMuted ? 0 : volumeSlider.value);
            MusicManager.Instance.ToggleMute(isMuted);
        }
        else
        {
            Debug.LogError("MusicManager instance is not set.");
        }

        volumeSlider.onValueChanged.AddListener(SetVolume);
        muteButton.onClick.AddListener(ToggleMute);

        resolutionDropdown.ClearOptions();
        foreach (var res in resolutions)
        {
            resolutionDropdown.options.Add(new TMP_Dropdown.OptionData($"{res.width} x {res.height}"));
        }

        resolutionDropdown.value = settings.resolutionIndex;
        resolutionDropdown.onValueChanged.AddListener(SetResolution);
        SetResolution(settings.resolutionIndex);
    }

    public void SetVolume(float volume)
    {
        if (!isMuted)
        {
            MusicManager.Instance.SetVolume(volume);
            previousVolume = volume;
            SaveSettings();
        }
    }

    public void ToggleMute()
    {
        isMuted = !isMuted;
        MusicManager.Instance.ToggleMute(isMuted);
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
        SettingsData settings = new SettingsData
        {
            volume = volumeSlider.value,
            isMuted = isMuted,
            resolutionIndex = resolutionDropdown.value
        };
        SaveManager.SaveData(settings);
    }

    public static SettingsData LoadSettings()
    {
        string path = Application.persistentDataPath + "/settings.json";
        if (File.Exists(path))
        {
            return JsonUtility.FromJson<SettingsData>(File.ReadAllText(path));
        }
        return new SettingsData { volume = 1f, isMuted = false, resolutionIndex = 3 };
    }
}



[System.Serializable]
public class SettingsData
{
    public float volume;
    public bool isMuted;
    public int resolutionIndex;
}
