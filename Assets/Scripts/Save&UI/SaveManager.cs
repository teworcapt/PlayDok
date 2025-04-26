// SaveManager.cs
using UnityEngine;
using System.IO;
using System.Collections.Generic;

[System.Serializable]
public class GameSaveData
{
    public int credits;
    public float permanentTimeBoost;
    public List<int> purchasedItems = new List<int>();
    public bool hasSeenTutorial = false;

    public int dayIndex;
    public int dailyPenalties;

    public int totalPatients;
    public int patientsCured;
    public int totalCuredPatients;

    public string CurrentDay
    {
        get
        {
            string[] daysOfWeek = {
                "Monday", "Tuesday", "Wednesday", "Thursday",
                "Friday", "Saturday", "Sunday"
            };
            if (dayIndex < 0 || dayIndex >= daysOfWeek.Length)
                return "Monday";
            return daysOfWeek[dayIndex];
        }
        set
        {
            string[] daysOfWeek = {
                "Monday", "Tuesday", "Wednesday", "Thursday",
                "Friday", "Saturday", "Sunday"
            };
            int idx = System.Array.IndexOf(daysOfWeek, value);
            dayIndex = idx >= 0 ? idx : 0;
        }
    }
}

public class SaveManager : MonoBehaviour
{
    private static readonly string[] DaysOfWeek = {
        "Monday", "Tuesday", "Wednesday", "Thursday",
        "Friday", "Saturday", "Sunday"
    };

    private static string SaveFolder => Application.persistentDataPath + "/Saves/";
    private static string GetSavePath(string fileName) =>
        Path.Combine(SaveFolder, fileName + ".json");

    private const string ProgressFileName = "playerProgress";
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
            if (int.TryParse(content, out int dayIdx))
                return dayIdx;
        }
        return 0;
    }

    public static string GetCurrentDay()
    {
        int idx = GetCurrentDayIndex();
        return (idx >= 0 && idx < DaysOfWeek.Length)
            ? DaysOfWeek[idx]
            : "Monday";
    }

    private static void SaveProgress(int dayIdx)
    {
        try
        {
            File.WriteAllText(ProgressPath, dayIdx.ToString());
        }
        catch (IOException e)
        {
            Debug.LogError($"Error saving progress: {e.Message}");
        }
    }

    public static string GetSaveFilePath(string day)
    {
        return GetSavePath("playerSave_" + day);
    }

    private static int RequiredPatientsForDay(string day)
    {
        switch (day)
        {
            case "Monday": return 2;
            case "Tuesday": return 3;
            case "Wednesday": return 5;
            case "Thursday": return 6;
            case "Friday": return 7;
            case "Saturday": return 6;
            case "Sunday": return 6;
            default:
                Debug.LogWarning($"Unknown day: {day}, defaulting to 7 patients");
                return 7;
        }
    }

    public static void SaveGame(GameSaveData data)
    {
        if (data == null) return;

        var stats = PlayerStats.Instance;
        data.totalPatients = stats.totalPatients;
        data.patientsCured = stats.patientsCured;
        data.totalCuredPatients = stats.totalCuredPatients;
        data.dailyPenalties = stats.dailyPenalties;
        data.credits = stats.totalEarnings;
        data.permanentTimeBoost = stats.timeBoostPermanent;
        data.purchasedItems = new List<int>(stats.itemsBought);

        string fileName = "playerSave_" + data.CurrentDay;
        string path = GetSavePath(fileName);

        try
        {
            File.WriteAllText(path, JsonUtility.ToJson(data, true));
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
        string fileName = "playerSave_" + dayName;
        string path = GetSavePath(fileName);

        GameSaveData data;
        if (!File.Exists(path))
        {
            data = new GameSaveData { dayIndex = dayIndex };
        }
        else
        {
            try
            {
                data = JsonUtility.FromJson<GameSaveData>(File.ReadAllText(path))
                       ?? new GameSaveData { dayIndex = dayIndex };
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error loading game for {dayName}: {e.Message}");
                data = new GameSaveData { dayIndex = dayIndex };
            }
        }

        var stats = PlayerStats.Instance;
        stats.totalPatients = data.totalPatients;
        stats.patientsCured = data.patientsCured;
        stats.totalCuredPatients = data.totalCuredPatients;
        stats.dailyPenalties = data.dailyPenalties;
        stats.totalEarnings = data.credits;
        stats.timeBoostPermanent = data.permanentTimeBoost;
        stats.itemsBought = new List<int>(data.purchasedItems);

        int required = RequiredPatientsForDay(dayName);
        stats.totalPatients = required;
        data.totalPatients = required;

        SaveProgress(dayIndex);
        return data;
    }

    public static void AdvanceDay()
    {
        int idx = GetCurrentDayIndex();
        if (idx < DaysOfWeek.Length - 1)
        {
            var data = LoadGame(idx);
            data.dayIndex = idx + 1;
            SaveGame(data);
        }
        else
        {
            Debug.Log("Reached last day (Sunday).");
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
        int idx = System.Array.IndexOf(DaysOfWeek, day);
        if (idx >= 0)
        {
            var data = LoadGame(idx);
            data.dayIndex = idx;
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
    private static string dataPath =
        Application.persistentDataPath + "/playerData.json";

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
