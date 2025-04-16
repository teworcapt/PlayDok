using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class RulebookManager : MonoBehaviour
{
    [Header("Monitor & Rulebook UI")]
    public GameObject monitorPanel;
    public GameObject rulebookUI;
    public Button monitorButton;
    public Transform diseaseList;
    public GameObject diseasePrefab;

    [Header("Disease Data")]
    public List<DiseaseData> diseases;

    [Header("Audio Settings UI")]
    public Slider musicVolumeSlider;
    public Button musicMuteButton;
    public TextMeshProUGUI musicMuteText;

    public Slider sfxVolumeSlider;
    public Button sfxMuteButton;
    public TextMeshProUGUI sfxMuteText;

    [Header("Resolution Settings")]
    public TMP_Dropdown resolutionDropdown;
    private Resolution[] resolutions = {
        new Resolution { width = 1280, height = 720 },
        new Resolution { width = 1366, height = 768 },
        new Resolution { width = 1600, height = 900 },
        new Resolution { width = 1920, height = 1080 }
    };

    [Header("Fullscreen Settings")]
    public Toggle fullscreenToggle;

    [Header("Current Status UI")]
    public TextMeshProUGUI penaltiesText;
    public TextMeshProUGUI patientsText;
    public TextMeshProUGUI creditsText;

    private bool isMonitorOpen = false;
    private bool isInterrogating = false;
    public static RulebookManager Instance { get; private set; }

    /* -------------------- Initialization -------------------- */
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        monitorButton.onClick.AddListener(ToggleMonitor);
        monitorPanel.SetActive(false);
        rulebookUI.SetActive(false);
        PopulateDiseases();

        // Load settings using SettingsManager.
        SettingsData settings = SettingsManager.LoadSettings();

        // Audio Settings Initialization
        if (AudioManager.Instance != null)
        {
            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.value = AudioManager.Instance.GetMusicVolume();
                musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
            }
            if (musicMuteButton != null)
            {
                musicMuteButton.onClick.AddListener(ToggleMusicMute);
                UpdateMusicMuteButtonText(AudioManager.Instance.IsMusicMuted());
            }

            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.value = AudioManager.Instance.GetSFXVolume();
                sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
            }
            if (sfxMuteButton != null)
            {
                sfxMuteButton.onClick.AddListener(ToggleSFXMute);
                UpdateSFXMuteButtonText(AudioManager.Instance.IsSFXMuted());
            }
        }
        else
        {
            Debug.LogError("AudioManager instance is not set.");
        }

        // Resolution Settings Initialization
        if (resolutionDropdown != null)
        {
            resolutionDropdown.ClearOptions();
            List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
            foreach (var res in resolutions)
            {
                options.Add(new TMP_Dropdown.OptionData($"{res.width} x {res.height}"));
            }
            resolutionDropdown.AddOptions(options);

            int savedResIndex = PlayerPrefs.HasKey("ResolutionIndex") ? PlayerPrefs.GetInt("ResolutionIndex") : settings.resolutionIndex;
            resolutionDropdown.value = savedResIndex;
            resolutionDropdown.onValueChanged.AddListener(SetResolution);
            SetResolution(savedResIndex);
        }

        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = settings.isFullscreen;
            fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
            Screen.fullScreen = settings.isFullscreen;
        }

        UpdateCurrentStatusUI();
    }

    private void Update()
    {
        if (isInterrogating || Time.timeScale == 0)
            return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            ToggleMonitor(!isMonitorOpen);
        }

        if (isMonitorOpen && Input.GetMouseButtonDown(0))
        {
            if (!IsPointerOverUIObject())
            {
                ToggleMonitor(false);
            }
        }

        UpdateCurrentStatusUI();
    }

    /* -------------------- Toggle Monitor & Rulebook -------------------- */
    public void ToggleMonitor()
    {
        isMonitorOpen = !isMonitorOpen;
        monitorPanel.SetActive(isMonitorOpen);
        rulebookUI.SetActive(isMonitorOpen);
    }

    public void ToggleMonitor(bool state)
    {
        isMonitorOpen = state;
        monitorPanel.SetActive(state);
        rulebookUI.SetActive(state);
    }

    /* -------------------- Disease List Population -------------------- */
    private void PopulateDiseases()
    {
        if (diseaseList == null || diseasePrefab == null)
            return;

        foreach (DiseaseData disease in diseases)
        {
            if (disease == null)
                continue;

            GameObject diseaseRow = Instantiate(diseasePrefab, diseaseList);
            TextMeshProUGUI[] textElements = diseaseRow.GetComponentsInChildren<TextMeshProUGUI>();

            if (textElements.Length >= 4)
            {
                textElements[0].text = disease.diseaseName;
                textElements[1].text = string.Join(", ", disease.symptoms);
                textElements[2].text = string.Join(", ", disease.tests);
                textElements[3].text = string.Join(", ", disease.treatments);
            }
        }
    }

    /* -------------------- Interrogation State -------------------- */
    private bool IsPointerOverUIObject()
    {
        return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
    }

    public void SetInterrogationState(bool isInterrogating)
    {
        this.isInterrogating = isInterrogating;
        if (monitorButton != null)
            monitorButton.interactable = !isInterrogating;
    }

    /* -------------------- Audio UI Methods -------------------- */
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

    private void UpdateMusicMuteButtonText(bool isMuted)
    {
        if (musicMuteText != null)
            musicMuteText.text = isMuted ? "Unmute" : "Mute";
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

    private void UpdateSFXMuteButtonText(bool isMuted)
    {
        if (sfxMuteText != null)
            sfxMuteText.text = isMuted ? "Unmute" : "Mute";
    }

    /* -------------------- Resolution Methods -------------------- */
    public void SetResolution(int index)
    {
        Resolution res = resolutions[index];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
        PlayerPrefs.SetInt("ResolutionIndex", index);
        PlayerPrefs.Save();
        SaveSettings();
    }

    /* -------------------- Fullscreen Method -------------------- */
    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        SaveSettings();
    }

    /* -------------------- Current Status UI -------------------- */
    private void UpdateCurrentStatusUI()
    {
        if (penaltiesText != null)
            penaltiesText.text = "Current Penalties: " + PlayerStats.Instance.dailyPenalties;
        if (patientsText != null)
            patientsText.text = "Current Patients: " + PlayerStats.Instance.totalPatients;
        if (creditsText != null)
            creditsText.text = "Current Credits: " + PlayerStats.Instance.totalEarnings;
    }

    /* -------------------- Save Settings -------------------- */
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
}
