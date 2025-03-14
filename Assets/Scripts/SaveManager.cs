using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;

[System.Serializable]
public class SaveData
{
    public string sceneName;
}

public class SaveManager : MonoBehaviour
{
    private static string saveFilePath => Path.Combine(Application.persistentDataPath, "savegame.json");

    public static void SaveGame()
    {
        SaveData data = new SaveData
        {
            sceneName = SceneManager.GetActiveScene().name
        };

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(saveFilePath, json);

        Debug.Log("Game Saved: " + saveFilePath);
    }

    public static void LoadGame()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            SceneManager.LoadScene(data.sceneName);
        }
        else
        {
            Debug.Log("No save file found.");
        }
    }

    public static bool SaveExists()
    {
        return File.Exists(saveFilePath);
    }
}
