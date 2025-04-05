using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

[System.Serializable]
public class DailySaveData
{
    public int day;
    public int credits;
    public int dailyPenalties;
    public List<int> purchasedItems = new List<int>();
}

public class LoadSaveManager : MonoBehaviour
{
    public static LoadSaveManager Instance { get; private set; }
    public static string CurrentLoadedDay { get; private set; }

    [Header("UI References")]
    public Transform saveListContainer;
    public GameObject saveEntryPrefab;
    public Button mainMenuButton;

    private static string SaveFolder => Application.persistentDataPath + "/Saves/";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        EnsureSaveFolderExists();
        LoadSaveList();
        mainMenuButton.onClick.AddListener(ReturnToMainMenu);
    }

    /* -------------------- Save Handling -------------------- */

    private static void EnsureSaveFolderExists()
    {
        if (!Directory.Exists(SaveFolder))
        {
            Directory.CreateDirectory(SaveFolder);
        }
    }

    private static string GetSavePath(string day) => Path.Combine(SaveFolder, $"day{day}.json");

    public static void SaveDayData(string day, int credits, int penalties, List<int> purchasedItems)
    {
        EnsureSaveFolderExists(); // Ensure the save folder exists before saving
        string path = GetSavePath(day);
        DailySaveData data = new DailySaveData
        {
            day = SaveManager.GetDayIndex() + 1,
            credits = credits,
            dailyPenalties = penalties,
            purchasedItems = purchasedItems
        };
        File.WriteAllText(path, JsonUtility.ToJson(data, true));
    }

    public static DailySaveData LoadDayData(string day)
    {
        string path = GetSavePath(day);
        if (File.Exists(path))
        {
            return JsonUtility.FromJson<DailySaveData>(File.ReadAllText(path));
        }
        return null;
    }

    /* -------------------- UI Handling -------------------- */

    public void LoadSaveList()
    {
        foreach (Transform child in saveListContainer)
            Destroy(child.gameObject);

        EnsureSaveFolderExists();
        string[] files = Directory.GetFiles(SaveFolder, "*.json");
        string[] daysOfWeek = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };

        foreach (string file in files)
        {
            DailySaveData data = JsonUtility.FromJson<DailySaveData>(File.ReadAllText(file));
            if (data == null || data.day < 1 || data.day > 7) continue;

            string dayOfWeek = daysOfWeek[data.day - 1];
            GameObject saveEntry = Instantiate(saveEntryPrefab, saveListContainer);
            saveEntry.GetComponent<SaveEntryUI>().SetData(dayOfWeek, data.credits, data.dailyPenalties, data.day);
        }
    }

    /* -------------------- Save Loading -------------------- */

    public void LoadSelectedDay(string day)
    {
        DailySaveData saveData = LoadDayData(day);
        if (saveData == null) return;

        CurrentLoadedDay = day;
        PlayerStats.Instance.SetCredits(saveData.credits);
        PlayerStats.Instance.SetPenalties(saveData.dailyPenalties);

        List<int> validItems = new List<int>();
        foreach (int itemID in saveData.purchasedItems)
        {
            ShopItem item = ShopManager.Instance.shopItems.Find(i => i.itemNumber == itemID);
            if (item == null) continue;

            if (item.itemType == ShopItemType.PermanentTimeBoost)
            {
                TimerManager.Instance.ApplyPermanentTimeBoost(item.timeBoostPermanent);
            }
            else if (item.itemType == ShopItemType.Cosmetic)
            {
                if (item.itemName == "Dog")
                {
                    ShopManager.Instance.SetAlpha(ShopManager.Instance.dogCanvasGroup, 1);
                }
                else if (item.itemName == "Potted Plant")
                {
                    ShopManager.Instance.SetAlpha(ShopManager.Instance.plantCanvasGroup, 1);
                }
            }

            validItems.Add(itemID);
        }

        PlayerStats.Instance.itemsBought = validItems;
        SceneManager.LoadScene("Gameplay");
    }

    /* -------------------- Scene Management -------------------- */

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1;
        AudioListener.pause = false;
        SceneManager.LoadScene("MainMenu");
    }
}
