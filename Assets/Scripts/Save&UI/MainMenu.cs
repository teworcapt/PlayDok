using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
	private void Start()
	{
		if (AudioManager.Instance != null)
		{
			AudioManager.Instance.PlayMainMenuMusic();
		}
	}

	public void PlayGame()
	{
		if (AudioManager.Instance != null)
		{
			AudioManager.Instance.PlayGameplayMusic();
		}
		AudioManager.Instance.PlayButtonClickSound();

        SceneManager.LoadScene("Gameplay");
	}

	public void LoadGame()
	{
		AudioManager.Instance.PlayButtonClickSound();

        SceneManager.LoadScene("LoadSave");
	}

	public void OpenSettings()
	{
		AudioManager.Instance.PlayButtonClickSound();

        SceneManager.LoadScene("Settings");
	}

	public void QuitGame()
	{
		Debug.Log("Quit Game");
		Application.Quit();
	}
}
