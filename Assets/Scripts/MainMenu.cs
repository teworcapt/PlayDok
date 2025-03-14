using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    private const string SaveKey = "SavedScene"; // Key for saved scene name

    public void PlayGame()
    {
        SceneManager.LoadScene("Gameplay"); // Change to your actual game scene
    }

    public void LoadGame()
    {
        if (PlayerPrefs.HasKey(SaveKey))
        {
            string savedScene = PlayerPrefs.GetString(SaveKey);
            SceneManager.LoadScene(savedScene);
        }
        else
        {
            Debug.Log("No saved game found.");
        }
    }

    public void OpenSettings()
    {
        Debug.Log("Open Settings Menu"); // Replace with your settings logic
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }
}
