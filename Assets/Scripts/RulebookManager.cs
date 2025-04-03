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
    private float previousVolume = 1f;
    private bool isMuted = false;

    [Header("Resolution Settings")]
    public TMP_Dropdown resolutionDropdown;
    private Resolution[] resolutions = {
        new Resolution { width = 1280, height = 720 },
        new Resolution { width = 1366, height = 768 },
        new Resolution { width = 1600, height = 900 },
        new Resolution { width = 1920, height = 1080 },
        new Resolution { width = 2560, height = 1440 },
        new Resolution { width = 3840, height = 2160 }
    };

    private bool isMonitorOpen = false;
    private bool isInterrogating = false;
    public static RulebookManager Instance { get; private set; }

    /* -------------------- Initialization -------------------- */
    private void Start()
    {
        monitorButton.onClick.AddListener(ToggleMonitor);
        monitorPanel.SetActive(false);
        rulebookUI.SetActive(false);
        PopulateDiseases();
        SetupSettings();
    }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
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

    /* -------------------- Settings Management -------------------- */
    private void SetupSettings()
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
}
