using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [Header("Pause Menu")]
    public GameObject pauseMenu;
    public Button pauseButton;
    public Button mainMenuButton;

    private bool isPaused = false;

    /* -------------------- Initialization -------------------- */
    void Start()
    {
        pauseButton.onClick.AddListener(TogglePause);
        mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        pauseMenu.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    /* -------------------- UI Buttons -------------------- */
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
}
