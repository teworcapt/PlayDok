using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    [Header("Pause Menu")]
    public GameObject pauseMenu;
    public Button pauseButton;
    public Button resumeButton;
    public Button mainMenuButton;

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

    [Header("Monitor Panel")]
    public GameObject monitorPanel;
    public Button monitorPanelButton;

    private bool isPaused = false;
    private bool isMonitorPanelOpen = false;

    void Start()
    {
        pauseButton.onClick.AddListener(TogglePause);
        resumeButton.onClick.AddListener(TogglePause);
        mainMenuButton.onClick.AddListener(ReturnToMainMenu);

        monitorPanelButton.onClick.AddListener(ToggleMonitorPanel);

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

        pauseMenu.SetActive(false);
        monitorPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            ToggleMonitorPanel();
        }

        if (isMonitorPanelOpen && Input.GetMouseButtonDown(0))
        {
            if (!IsPointerOverUIObject())
            {
                ToggleMonitorPanel(false);
            }
        }
    }

    // === PAUSE MENU FUNCTIONS ===
    public void TogglePause()
    {
        isPaused = !isPaused;
        pauseMenu.SetActive(isPaused);
        Time.timeScale = isPaused ? 0 : 1;
        AudioListener.pause = isPaused;
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1;
        AudioListener.pause = false;
        SceneManager.LoadScene("MainMenu");
    }

    // === AUDIO SETTINGS FUNCTIONS ===
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

    // === RESOLUTION SETTINGS FUNCTIONS ===
    public void SetResolution(int index)
    {
        Resolution res = resolutions[index];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
        PlayerPrefs.SetInt("ResolutionIndex", index);
        PlayerPrefs.Save();
    }

    // === MONITOR PANEL FUNCTIONS ===
    public void ToggleMonitorPanel()
    {
        isMonitorPanelOpen = !isMonitorPanelOpen;
        monitorPanel.SetActive(isMonitorPanelOpen);
    }

    public void ToggleMonitorPanel(bool state)
    {
        isMonitorPanelOpen = state;
        monitorPanel.SetActive(state);
    }

    private bool IsPointerOverUIObject()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var result in results)
        {
            if (result.gameObject == gameObject || result.gameObject == monitorPanel)
            {
                return true;
            }
        }

        return false;
    }
}
