using UnityEngine;
using System.IO;

[System.Serializable]
public class PlayerData
{
    public float extraTimePercentage;
    public int credits;
}

public class SaveManager : MonoBehaviour
{
    private static string savePath => Application.persistentDataPath + "/playerSave.json";

    public static void SaveData(PlayerData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
    }

    public static PlayerData LoadData()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            return JsonUtility.FromJson<PlayerData>(json);
        }
        return new PlayerData();
    }
}
