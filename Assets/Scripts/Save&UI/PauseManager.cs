using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [Header("Pause Menu")]
    public GameObject pauseMenu;
    public Button pauseButton;
    public Button mainMenuButton;
    public Button viewTutorialButton;

    [Header("Tutorial Box")]
    public GameObject tutorialBox;
    public Button closeTutorialButton;

    private bool isPaused = false;

    void Start()
    {
        pauseButton.onClick.AddListener(TogglePause);
        mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        closeTutorialButton.onClick.AddListener(CloseTutorial);
        viewTutorialButton.onClick.AddListener(ShowTutorial);
        pauseMenu.SetActive(false);

        PlayerData data = SaveManager.LoadData();

        // Check if it's Monday AND the player hasn't seen the tutorial
        string currentDay = SaveManager.GetCurrentDay();
        bool isMonday = currentDay == "Monday";

        if (isMonday && !data.hasSeenTutorial)
        {
            ShowTutorial();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !tutorialBox.activeSelf)
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        pauseMenu.SetActive(isPaused);
        Time.timeScale = isPaused ? 0 : 1;
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("MainMenu");
    }

    private void ShowTutorial()
    {
        pauseMenu.SetActive(false);
        tutorialBox.SetActive(true);
        Time.timeScale = 0;
    }

    private void CloseTutorial()
    {
        tutorialBox.SetActive(false);
        Time.timeScale = 1;
        if (!PlayerPrefs.HasKey("HasSeenTutorial"))
        {
            PlayerPrefs.SetInt("HasSeenTutorial", 1);
            PlayerPrefs.Save();
        }
    }
}