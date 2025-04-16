using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;
public class LoadSaveManager : MonoBehaviour
{
    public static LoadSaveManager Instance { get; private set; }
    public static string CurrentLoadedDay { get; private set; }
    public Transform saveListContainer;
    public GameObject saveEntryPrefab;
    public Button mainMenuButton;
    public Button deleteAllSavesButton;
    private static readonly string[] DaysOfWeek = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
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
        if (PlayerStats.Instance == null)
        {
            Debug.LogError("PlayerStats instance is not set! Creating one in Start().");
            GameObject go = new GameObject("PlayerStats");
            go.AddComponent<PlayerStats>();
        }
        LoadSaveList();
        mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        deleteAllSavesButton.onClick.AddListener(DeleteAllSaves);
    }
    public void LoadSaveList()
    {
        foreach (Transform child in saveListContainer)
            Destroy(child.gameObject);
        for (int i = 0; i < DaysOfWeek.Length; i++)
        {
            string dayName = DaysOfWeek[i];
            string savePath = SaveManager.GetSaveFilePath(dayName);
            bool saveExists = File.Exists(savePath);
            GameSaveData data = SaveManager.LoadGame(i);
            GameObject saveEntry = Instantiate(saveEntryPrefab, saveListContainer);
            SaveEntryUI entryUI = saveEntry.GetComponent<SaveEntryUI>();
            entryUI.SetData(dayName, data.credits, data.dailyPenalties, i + 1);
            entryUI.SetLoadButtonInteractable(saveExists);
        }
    }
    public void LoadSelectedDay(string day)
    {
        int dayIndex = System.Array.IndexOf(DaysOfWeek, day);
        if (dayIndex == -1)
        {
            Debug.LogError("Invalid day selected: " + day);
            return;
        }
        GameSaveData saveData = SaveManager.LoadGame(dayIndex);
        if (saveData == null)
        {
            Debug.LogError("No save data found for " + day);
            return;
        }
        PlayerStats.Instance.SetCredits(saveData.credits);
        PlayerStats.Instance.SetPenalties(saveData.dailyPenalties);
        PlayerStats.Instance.itemsBought = new List<int>(saveData.purchasedItems);
        CurrentLoadedDay = saveData.CurrentDay;
        if (ShopManager.Instance != null)
        {
            ShopManager.Instance.ResetDailyItems();
        }

        SceneManager.LoadScene("Gameplay");

    }
    public void DeleteAllSaves()
    {
        // Delete all save files
        string saveFolder = Application.persistentDataPath + "/Saves/";
        if (Directory.Exists(saveFolder))
        {
            Directory.Delete(saveFolder, true);
            Directory.CreateDirectory(saveFolder);
            Debug.Log("All save files deleted.");
        }

        // Reset permanent time boosts in PlayerStats
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.ResetPermanentTimeBoosts();
            PlayerStats.Instance.itemsBought.Clear();
            Debug.Log("Permanent time boosts and purchased items reset.");
        }

        // Reset timer if it exists in the scene
        if (TimerManager.Instance != null)
        {
            TimerManager.Instance.ResetPermanentTimeBoosts();
            Debug.Log("Timer manager permanent boosts reset.");
        }

        LoadSaveList();
    }
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1;
        AudioListener.pause = false;
        SceneManager.LoadScene("MainMenu");
    }
}