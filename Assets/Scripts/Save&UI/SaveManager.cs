using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class GameSaveData
{
    public int credits;
    public float permanentTimeBoost;
    public List<int> purchasedItems = new List<int>();
    public bool hasSeenTutorial = false;

    public int dayIndex;
    public int dailyPenalties;

    public string CurrentDay
    {
        get
        {
            string[] daysOfWeek = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
            if (dayIndex < 0 || dayIndex >= daysOfWeek.Length)
                return "Monday";
            return daysOfWeek[dayIndex];
        }
        set
        {
            string[] daysOfWeek = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
            int index = System.Array.IndexOf(daysOfWeek, value);
            dayIndex = index >= 0 ? index : 0;
        }
    }
}

public class SaveManager : MonoBehaviour
{
    private static readonly string[] DaysOfWeek = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
    private static string SaveFolder => Application.persistentDataPath + "/Saves/";
    private static string GetSavePath(string fileName) => Path.Combine(SaveFolder, fileName + ".json");
    private const string ProgressFileName = "playerProgress";
    // A property for the progress file path:
    private static string ProgressPath => GetSavePath(ProgressFileName);

    private void Awake()
    {
        if (!Directory.Exists(SaveFolder))
            Directory.CreateDirectory(SaveFolder);
    }

    public static int GetCurrentDayIndex()
    {
        if (File.Exists(ProgressPath))
        {
            string content = File.ReadAllText(ProgressPath);
            if (int.TryParse(content, out int dayIndex))
                return dayIndex;
        }
        return 0; // Default to Monday (index 0)
    }

    public static string GetCurrentDay()
    {
        int dayIndex = GetCurrentDayIndex();
        if (dayIndex >= 0 && dayIndex < DaysOfWeek.Length)
            return DaysOfWeek[dayIndex];
        return "Monday";
    }

    public static void SaveGame(GameSaveData data)
    {
        if (data == null) return;
        string saveFileName = "playerSave_" + data.CurrentDay;
        string savePath = GetSavePath(saveFileName);
        try
        {
            File.WriteAllText(savePath, JsonUtility.ToJson(data, true));
            SaveProgress(data.dayIndex);
        }
        catch (IOException e)
        {
            Debug.LogError($"Error saving game for {data.CurrentDay}: {e.Message}");
        }
    }

    public static GameSaveData LoadGame(int dayIndex)
    {
        string dayName = DaysOfWeek[dayIndex];
        string saveFileName = "playerSave_" + dayName;
        string savePath = GetSavePath(saveFileName);
        if (!File.Exists(savePath))
            return new GameSaveData { dayIndex = dayIndex };

        try
        {
            string json = File.ReadAllText(savePath);
            GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);
            // Ensure the current day string and index match:
            data.CurrentDay = dayName;
            return data ?? new GameSaveData { dayIndex = dayIndex };
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading game for {dayName}: {e.Message}");
            return new GameSaveData { dayIndex = dayIndex };
        }
    }
    public static string GetSaveFilePath(string day)
    {
        return GetSavePath("playerSave_" + day);
    }

    private static void SaveProgress(int dayIndex)
    {
        try
        {
            File.WriteAllText(ProgressPath, dayIndex.ToString());
        }
        catch (IOException e)
        {
            Debug.LogError($"Error saving progress: {e.Message}");
        }
    }

    public static void AdvanceDay()
    {
        int currentIndex = GetCurrentDayIndex();
        if (currentIndex < DaysOfWeek.Length - 1)
        {
            GameSaveData data = LoadGame(currentIndex);
            data.dayIndex = currentIndex + 1;
            SaveGame(data);
        }
        else
        {
            Debug.Log("Game has reached the last day (Sunday). Save file is complete.");
        }
    }

    public static void SaveData(SettingsData data)
    {
        string path = Application.persistentDataPath + "/settings.json";
        try
        {
            File.WriteAllText(path, JsonUtility.ToJson(data));
        }
        catch (IOException e)
        {
            Debug.LogError($"Error saving settings: {e.Message}");
        }
    }


    public static void SetDay(string day)
    {
        int index = System.Array.IndexOf(DaysOfWeek, day);
        if (index >= 0)
        {
            GameSaveData data = LoadGame(index);
            data.dayIndex = index;
            SaveGame(data);
        }
        else
        {
            Debug.LogError("Invalid day: " + day);
        }
    }
}

[System.Serializable]
public class PlayerPersistentData
{
    public float permanentTimeBoost;
}

public static class PersistentDataManager
{
    private static string dataPath = Application.persistentDataPath + "/playerData.json";
    public static PlayerPersistentData LoadData()
    {
        if (!File.Exists(dataPath))
            return new PlayerPersistentData();
        string json = File.ReadAllText(dataPath);
        return JsonUtility.FromJson<PlayerPersistentData>(json);
    }
    public static void SaveData(PlayerPersistentData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(dataPath, json);
    }
}

