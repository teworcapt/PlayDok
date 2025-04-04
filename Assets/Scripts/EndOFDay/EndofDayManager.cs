using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class EndOfDayManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text dayText;
    [SerializeField] private TMP_Text totalPatientsText;
    [SerializeField] private TMP_Text patientsCuredText;
    [SerializeField] private TMP_Text penaltyText;
    [SerializeField] private TMP_Text totalPayText;

    private int netEarnings;

    /* -------------------- Initialization -------------------- */

    private void Start()
    {
        if (PlayerStats.Instance == null)
        {
            Debug.LogError("PlayerStats.Instance is NULL! Ensure PlayerStats exists in the first scene.");
            return;
        }
        DisplayStats();
        ApplyEarnings();
    }

    /* -------------------- Display Methods -------------------- */

    private void DisplayStats()
    {
        var stats = PlayerStats.Instance;
        totalPatientsText.text = stats.totalPatients.ToString();
        patientsCuredText.text = stats.patientsCured.ToString();
        penaltyText.text = stats.dailyPenalties.ToString();

        netEarnings = stats.totalEarnings - stats.dailyPenalties;
        totalPayText.text = netEarnings.ToString();

        UpdateDayText();
    }

    private void UpdateDayText()
    {
        if (dayText != null)
            dayText.text = $"Day {SaveManager.GetCurrentDay()}";
    }

    /* -------------------- Earnings & Save -------------------- */

    private void ApplyEarnings()
    {
        PlayerData playerData = SaveManager.LoadData();
        if (playerData == null)
        {
            Debug.LogError("Failed to load player data. Cannot apply earnings.");
            return;
        }

        playerData.SetCredits(playerData.GetCredits() + netEarnings);
        SaveManager.SaveData(playerData);

        string dayToSave = !string.IsNullOrEmpty(LoadSaveManager.CurrentLoadedDay)
                                ? LoadSaveManager.CurrentLoadedDay
                                : SaveManager.GetCurrentDay();

        LoadSaveManager.SaveDayData(dayToSave, playerData.GetCredits(), PlayerStats.Instance.dailyPenalties, playerData.purchasedItems);
    }

    /* -------------------- Scene Transition -------------------- */

    public void OnNextDay()
    {
        SaveManager.AdvanceDay();
        PlayerStats.Instance.ResetDailyStats();
        SceneManager.LoadScene("Gameplay");
    }
}
