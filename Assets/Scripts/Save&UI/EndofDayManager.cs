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

    private void Start()
    {
        DisplayStats();
        ApplyEarnings();
    }

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
        {
            dayText.text = $"Day {SaveManager.GetCurrentDay()}";
        }
    }

    private void ApplyEarnings()
    {
        int currentDayIndex = SaveManager.GetCurrentDayIndex();
        GameSaveData gameData = SaveManager.LoadGame(currentDayIndex);
        if (gameData == null)
        {
            Debug.LogError("Failed to load game data. Cannot apply earnings.");
            return;
        }

        // Update credits with net earnings.
        gameData.credits += netEarnings;

        // Persist changes using the unified save manager.
        SaveManager.SaveGame(gameData);

        // Update PlayerStats with the new credit value.
        PlayerStats.Instance.SetCredits(gameData.credits);
    }

    public void OnNextDay()
    {
        if (ShopManager.Instance != null)
        {
            ShopManager.Instance.ResetDailyItems();
        }

        if (SaveManager.GetCurrentDay() == "Sunday")
        {
            SceneManager.LoadScene("EndingScene");
        }
        else
        {
            // Advance to the next day in the unified save data.
            SaveManager.AdvanceDay();
            PlayerStats.Instance.ResetDailyStats();
            SceneManager.LoadScene("Gameplay");
        }
    }
}
