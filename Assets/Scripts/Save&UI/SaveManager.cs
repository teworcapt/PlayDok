using UnityEngine;
using System.IO;

[System.Serializable]
public class PlayerData
{
    public int credits;
    public float permanentTimeBoost;
    public System.Collections.Generic.List<int> purchasedItems = new System.Collections.Generic.List<int>();
    public string currentDay = "Monday";

    public int GetCredits() => credits;
    public void SetCredits(int amount) => credits = amount;
}

public class SaveManager : MonoBehaviour
{
    private static readonly string[] DaysOfWeek = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
    private static string SavePath => Application.persistentDataPath + "/playerSave.json";

    /* -------------------- Save & Load -------------------- */

    public static void SaveData(PlayerData data)
    {
        if (data == null) return;
        try { File.WriteAllText(SavePath, JsonUtility.ToJson(data, true)); }
        catch (IOException e) { Debug.LogError($"Error saving game: {e.Message}"); }
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

    /* -------------------- Day Management -------------------- */

    public static string GetCurrentDay()
    {
        return LoadData().currentDay;
    }

    public static int GetDayIndex()
    {
        PlayerData data = LoadData();
        return System.Array.IndexOf(DaysOfWeek, data.currentDay);
    }

    public static void SetDayIndex(int dayIndex)
    {
        if (dayIndex >= 0 && dayIndex < DaysOfWeek.Length)
        {
            PlayerData data = LoadData();
            data.currentDay = DaysOfWeek[dayIndex];
            SaveData(data);
        }
    }

    public static void AdvanceDay()
    {
        int currentIndex = GetDayIndex();
        if (currentIndex < DaysOfWeek.Length - 1)
            SetDayIndex(currentIndex + 1);
        else
            Debug.Log("Game has reached the last day (Sunday). Save file is complete.");
    }

    public static void SaveData(SettingsData data)
    {
        string path = Application.persistentDataPath + "/settings.json";
        File.WriteAllText(path, JsonUtility.ToJson(data));
    }
}
