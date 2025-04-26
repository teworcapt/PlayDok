using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProgressManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider progressBar;
    [SerializeField] private TextMeshProUGUI progressLabel;

    private int totalPatients;
    private int patientsCured;

    public static ProgressManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        var data = SaveManager.LoadGame(SaveManager.GetCurrentDayIndex());

        int todayIdx = SaveManager.GetCurrentDayIndex();
        if (data.dayIndex != todayIdx)
        {
            PlayerStats.Instance.ResetDailyStats();
            string today = SaveManager.GetCurrentDay();
            PlayerStats.Instance.totalPatients = GetRequiredPatientsForDay(today);

            data.dayIndex = todayIdx;
            data.totalPatients = PlayerStats.Instance.totalPatients;
            data.patientsCured = 0;
            SaveManager.SaveGame(data);
        }

        // 3) Pull for UI
        totalPatients = PlayerStats.Instance.totalPatients;
        patientsCured = PlayerStats.Instance.patientsCured;
        UpdateUI();
    }

    public void PatientCured(bool correctDiagnosis, bool correctTreatment)
    {
        if (!correctDiagnosis || !correctTreatment) return;

        PlayerStats.Instance.AddCuredPatient();

        var data = SaveManager.LoadGame(SaveManager.GetCurrentDayIndex());
        SaveManager.SaveGame(data);

        patientsCured = PlayerStats.Instance.patientsCured;
        UpdateUI();

        if (patientsCured >= totalPatients)
            Debug.Log("Day completed! All required patients cured.");
    }

    public void PatientCured()
    {
        Debug.Log("PatientCured called without diagnosis info. No progress change.");
    }

    private void UpdateUI()
    {
        progressBar.maxValue = totalPatients;
        progressBar.value = patientsCured;
        if (progressLabel != null)
            progressLabel.text = $"{patientsCured} / {totalPatients}";
    }

    private int GetRequiredPatientsForDay(string day)
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

    public void ResetProgress(string day)
    {
        PlayerStats.Instance.ResetDailyStats();
        totalPatients = GetRequiredPatientsForDay(day);
        PlayerStats.Instance.totalPatients = totalPatients;

        var data = SaveManager.LoadGame(SaveManager.GetCurrentDayIndex());
        data.totalPatients = totalPatients;
        data.patientsCured = 0;
        SaveManager.SaveGame(data);

        UpdateUI();
    }

    public void SetCurrentDay(string day)
    {
        if (string.IsNullOrEmpty(day)) return;
        int req = GetRequiredPatientsForDay(day);
        PlayerStats.Instance.ResetDailyStats();
        PlayerStats.Instance.totalPatients = req;

        var data = SaveManager.LoadGame(
            System.Array.IndexOf(
                new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" },
                day
            )
        );
        data.totalPatients = req;
        data.dayIndex = System.Array.IndexOf(
            new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" },
            day
        );
        SaveManager.SaveGame(data);

        totalPatients = req;
        patientsCured = 0;
        UpdateUI();
    }
}
