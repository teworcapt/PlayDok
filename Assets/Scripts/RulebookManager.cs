using UnityEngine;
using UnityEngine.UI;
using TMPro;
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

    [Header("Audio Settings")]
    public Slider volumeSlider;
    public Button muteButton;
    private float previousVolume;
    private bool isMuted = false;

    [Header("Resolution Settings")]
    public TMP_Dropdown resolutionDropdown;
    private Resolution[] resolutions = {
            new Resolution { width = 1280, height = 720 },
            new Resolution { width = 1366, height = 768 },
            new Resolution { width = 1600, height = 900 },
            new Resolution { width = 1920, height = 1080 }
        };

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

        SettingsData settings = SettingsManager.LoadSettings();
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

        UpdateCurrentStatusUI();
    }



    private void Update()
    {
        if (isInterrogating || Time.timeScale == 0) return;

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

        // Update current status UI in real-time
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
        if (diseaseList == null || diseasePrefab == null) return;

        foreach (DiseaseData disease in diseases)
        {
            if (disease == null) continue;

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
        monitorButton.interactable = !isInterrogating;
    }

    /* -------------------- Volume Settings -------------------- */
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

    /* -------------------- Resolution Settings -------------------- */
    public void SetResolution(int index)
    {
        Resolution res = resolutions[index];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
        SaveSettings();
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

    /* -------------------- Current Status UI -------------------- */
    private void UpdateCurrentStatusUI()
    {
        penaltiesText.text = "Current Penalties: " + PlayerStats.Instance.dailyPenalties;
        patientsText.text = "Current Patients: " + PlayerStats.Instance.totalPatients;
        creditsText.text = "Current Credits: " + PlayerStats.Instance.totalEarnings;
    }
}
