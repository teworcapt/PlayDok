using UnityEngine;
using System.IO;

[System.Serializable]
public class PlayerData
{
    public int credits;
    public float permanentTimeBoost;
    public System.Collections.Generic.List<int> purchasedItems = new System.Collections.Generic.List<int>();
    public string currentDay = "Monday";

    public bool hasSeenTutorial = false;

    public int GetCredits() => credits;
    public void SetCredits(int amount) => credits = amount;
}
public class SaveManager : MonoBehaviour
{
    private static readonly string[] DaysOfWeek = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };

    private static string GetSavePath(string day) => Application.persistentDataPath + "/playerSave_" + day + ".json";
    private static string ProgressPath => Application.persistentDataPath + "/playerProgress.json";

    public static PlayerData LoadData() => LoadData(GetCurrentDay());
    public static int GetDayIndex() => GetDayIndex(GetCurrentDay());

    public static void SaveData(PlayerData data)
    {
        if (data == null) return;
        string savePath = GetSavePath(data.currentDay);
        try
        {
            File.WriteAllText(savePath, JsonUtility.ToJson(data, true));
            SaveProgress(data.currentDay);
        }
        catch (IOException e)
        {
            Debug.LogError($"Error saving game for {data.currentDay}: {e.Message}");
        }
    }

    public static PlayerData LoadData(string day)
    {
        string savePath = GetSavePath(day);
        if (!File.Exists(savePath)) return new PlayerData();
        try
        {
            string json = File.ReadAllText(savePath);
            return JsonUtility.FromJson<PlayerData>(json) ?? new PlayerData();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading game for {day}: {e.Message}");
            return new PlayerData();
        }
    }

    public static PlayerData LoadCurrentData()
    {
        return LoadData("Monday");
    }

    public static int GetDayIndex(string day)
    {
        return System.Array.IndexOf(DaysOfWeek, day);
    }

    public static void SetDay(string day)
    {
        if (System.Array.IndexOf(DaysOfWeek, day) >= 0)
        {
            PlayerData data = LoadData(day);
            data.currentDay = day;
            SaveData(data);
        }
        else
        {
            Debug.LogError("Invalid day: " + day);
        }
    }

    public static void AdvanceDay()
    {
        PlayerData data = LoadData(GetCurrentDay());
        int currentIndex = GetDayIndex(data.currentDay);
        if (currentIndex >= 0 && currentIndex < DaysOfWeek.Length - 1)
        {
            string newDay = DaysOfWeek[currentIndex + 1];
            data.currentDay = newDay;
            SaveData(data);
        }
        else
        {
            Debug.Log("Game has reached the last day (Sunday). Save file is complete.");
        }
    }

    public static PlayerData LoadSelectedDay(string selectedDay)
    {
        int selectedIndex = GetDayIndex(selectedDay);
        if (selectedIndex == -1)
        {
            Debug.LogError("Invalid day selected: " + selectedDay);
            return new PlayerData();
        }
        DeleteSavesAfterDay(selectedIndex);
        PlayerData data = LoadData(selectedDay);
        data.currentDay = selectedDay;
        SaveProgress(selectedDay);
        return data;
    }

    private static void DeleteSavesAfterDay(int selectedDayIndex)
    {
        for (int i = selectedDayIndex + 1; i < DaysOfWeek.Length; i++)
        {
            string day = DaysOfWeek[i];
            string path = GetSavePath(day);
            if (File.Exists(path))
            {
                try
                {
                    File.Delete(path);
                    Debug.Log("Deleted save file for " + day + " at " + path);
                }
                catch (IOException e)
                {
                    Debug.LogError($"Error deleting save file for {day}: {e.Message}");
                }
            }
        }
    }

    public static void SaveData(SettingsData data)
    {
        string path = Application.persistentDataPath + "/settings.json";
        File.WriteAllText(path, JsonUtility.ToJson(data));
    }

    // Persists the current day in a progress file.
    private static void SaveProgress(string day)
    {
        try { File.WriteAllText(ProgressPath, day); }
        catch (IOException e) { Debug.LogError($"Error saving progress: {e.Message}"); }
    }

    public static string GetCurrentDay()
    {
        if (File.Exists(ProgressPath))
            return File.ReadAllText(ProgressPath);
        else
            return "Monday";
    }
}
