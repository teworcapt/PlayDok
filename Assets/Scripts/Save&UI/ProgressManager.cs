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
        // Initialize patients cured from PlayerStats or set to 0 if not already set
        patientsCured = PlayerStats.Instance.patientsCured;

        // Get the current day from LoadSaveManager if available, otherwise fallback to SaveManager
        string currentDay = LoadSaveManager.CurrentLoadedDay;
        if (string.IsNullOrEmpty(currentDay))
        {
            currentDay = SaveManager.GetCurrentDay();
            Debug.Log($"Using day from SaveManager: {currentDay}");
        }
        else
        {
            Debug.Log($"Using day from LoadSaveManager: {currentDay}");
        }

        totalPatients = GetRequiredPatientsForDay(currentDay);
        UpdateUI();
    }

    public void PatientCured(bool correctDiagnosis, bool correctTreatment)
    {
        if (correctDiagnosis && correctTreatment)
        {
            patientsCured++;
            PlayerStats.Instance.patientsCured = patientsCured;
            UpdateUI();

            // Check if day is completed
            if (patientsCured >= totalPatients)
            {
                Debug.Log("Day completed! All required patients cured.");
                // You could trigger day completion events here
            }
        }
    }

    public void PatientCured()
    {
        // This is kept for backward compatibility but won't increment progress
        Debug.Log("PatientCured called without diagnosis information. Progress not incremented.");
    }

    private void UpdateUI()
    {
        progressBar.maxValue = totalPatients;
        progressBar.value = patientsCured;
        if (progressLabel != null)
        {
            progressLabel.text = $"{patientsCured} / {totalPatients}";
        }
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

    // Public method to reset progress when needed
    public void ResetProgress(string day)
    {
        patientsCured = 0;
        PlayerStats.Instance.patientsCured = 0;
        totalPatients = GetRequiredPatientsForDay(day);
        UpdateUI();
    }

    // Public method to manually set the current day and update requirements
    public void SetCurrentDay(string day)
    {
        if (!string.IsNullOrEmpty(day))
        {
            totalPatients = GetRequiredPatientsForDay(day);
            UpdateUI();
        }
    }
}