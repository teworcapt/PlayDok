using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class EndOfDayManager : MonoBehaviour
{
    [SerializeField] private TMP_Text dayText;
    [SerializeField] private TMP_Text totalPatientsText;
    [SerializeField] private TMP_Text patientsCuredText;
    [SerializeField] private TMP_Text penaltyText;
    [SerializeField] private TMP_Text totalPayText;

    private int netEarnings;

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

    private void DisplayStats()
    {
        PlayerStats stats = PlayerStats.Instance;
        if (stats == null) return;

        totalPatientsText.text = stats.totalPatients.ToString();
        patientsCuredText.text = stats.patientsCured.ToString();
        penaltyText.text = stats.penalties.ToString();

        netEarnings = stats.totalEarnings - stats.penalties;
        totalPayText.text = netEarnings.ToString();

        UpdateDayText();
    }

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
    }

    private void UpdateDayText()
    {
        if (dayText != null)
            dayText.text = SaveManager.GetCurrentDay();
    }

    public void OnNextDay()
    {
        PlayerData playerData = SaveManager.LoadData();
        if (playerData == null)
        {
            Debug.LogError("Failed to load player data. Cannot proceed to the next day.");
            return;
        }

        if (playerData.currentDay < 7)
        {
            SaveManager.AdvanceDay();
            PlayerStats.Instance.ResetDailyStats();
            SceneManager.LoadScene("Gameplay");
        }
        else
        {
            Debug.Log("Game Over! Reached Sunday.");
            SceneManager.LoadScene("MainMenu");
        }
    }
}
