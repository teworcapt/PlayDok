using UnityEngine;
using System.IO;
using System.Collections.Generic;

[System.Serializable]
public class PlayerData
{
    private int credits;
    public float extraTimePercentage;
    public float permanentTimeBoost;
    public List<int> purchasedItems = new List<int>();
    public int currentDay;

    public PlayerData()
    {
        credits = 0;
        extraTimePercentage = 0f;
        permanentTimeBoost = 0f;
        currentDay = 1;
    }

    public int GetCredits() => credits;
    public void SetCredits(int amount) => credits = amount;
}

public class SaveManager : MonoBehaviour
{
    private static string SavePath => Application.persistentDataPath + "/playerSave.json";

    public static void SaveData(PlayerData data)
    {
        if (data == null) return;

        try
        {
            File.WriteAllText(SavePath, JsonUtility.ToJson(data, true));
        }
        catch (IOException e)
        {
            Debug.LogError($"Error saving game: {e.Message}");
        }
    }

    public static PlayerData LoadData()
    {
        if (!File.Exists(SavePath)) return new PlayerData();

        try
        {
            string json = File.ReadAllText(SavePath);
            return JsonUtility.FromJson<PlayerData>(json) ?? new PlayerData();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading game: {e.Message}");
            return new PlayerData();
        }
    }

    public static void AdvanceDay()
    {
        PlayerData data = LoadData();
        if (data.currentDay < 7)
        {
            data.currentDay++;
            SaveData(data);
        }
        else
        {
            Debug.Log("Game has reached the last day (Sunday). Save file is complete.");
        }
    }

    public static string GetCurrentDay()
    {
        int dayIndex = LoadData().currentDay - 1;
        string[] daysOfWeek = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
        return daysOfWeek[Mathf.Clamp(dayIndex, 0, 6)];
    }
}
