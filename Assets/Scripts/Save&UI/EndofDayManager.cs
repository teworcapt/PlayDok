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

        if (dayText != null)
            dayText.text = $"Day {SaveManager.GetCurrentDay()}";
    }

    private void ApplyEarnings()
    {
        PlayerStats.Instance.AddCredits(netEarnings);

        var stub = new GameSaveData
        {
            dayIndex = SaveManager.GetCurrentDayIndex()
        };
        SaveManager.SaveGame(stub);
    }

    public void OnNextDay()
    {
        if (ShopManager.Instance != null)
            ShopManager.Instance.ResetDailyItems();

        int idx = SaveManager.GetCurrentDayIndex();
        if (idx == 6)
        {
            SceneManager.LoadScene("Ending");
        }
        else
        {
            SaveManager.AdvanceDay();
            PlayerStats.Instance.ResetDailyStats();
            SceneManager.LoadScene("Gameplay");
        }
    }
}
