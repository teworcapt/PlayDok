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
        PlayerStats.Instance.patientsCured = 0;
        string currentDay = SaveManager.GetCurrentDay();
        totalPatients = GetRequiredPatientsForDay(currentDay);
        patientsCured = PlayerStats.Instance.patientsCured;
        UpdateUI();
    }

    public void PatientCured(bool correctDiagnosis, bool correctTreatment)
    {
        if (correctDiagnosis && correctTreatment)
        {
            patientsCured++;
            PlayerStats.Instance.patientsCured = patientsCured;
            UpdateUI();
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
            default: return 7;
        }
    }
}