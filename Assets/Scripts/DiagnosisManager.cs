using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class DiagnosisManager : MonoBehaviour
{
    public static DiagnosisManager Instance { get; private set; }

    [Header("Managers")]
    public DiseaseManager diseaseManager;
    public PatientManager patientManager;
    public DiagnosticsManager diagnosticsManager;

    [Header("Available Patients")]
    public List<PatientData> availablePersonalities;

    private PatientData currentPatient;
    private DiseaseData assignedDisease;
    private List<string> patientSymptoms = new List<string>();

    [Header("UI Elements")]
    public TMP_Dropdown diseaseChoices;
    public TMP_Dropdown treatmentChoices;
    public TextMeshProUGUI symptomsText;
    public TextMeshProUGUI dailyPenaltyText;
    public TextMeshProUGUI totalPenaltyText;
    public Button submitButton;

    private string selectedDisease;
    private string selectedTreatment;

    /* -------------------- Initialization -------------------- */
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        diseaseChoices.onValueChanged.AddListener(delegate { OnDropdownChanged(); });
        treatmentChoices.onValueChanged.AddListener(delegate { OnDropdownChanged(); });
        submitButton.onClick.AddListener(CheckDiagnosis);

        SpawnNextPatient();
    }

    /* -------------------- Patient Handling -------------------- */
    public void SpawnNextPatient()
    {
        if (availablePersonalities.Count == 0)
        {
            Debug.LogError("No patients available!");
            return;
        }

        DiagnosticsManager.Instance.ResetDiagnostics();

        currentPatient = availablePersonalities[Random.Range(0, availablePersonalities.Count)];
        currentPatient.Initialize();

        AssignRandomDisease();
        patientManager.UpdatePatientUI(currentPatient, patientSymptoms);
        PopulateDropdowns();

        if (patientManager.rulebook != null)
            patientManager.rulebook.SetActive(false);

        patientManager.EnableInterrogateButton();
    }

    private void AssignRandomDisease()
    {
        if (diseaseManager.diseaseDataList.Count == 0)
        {
            Debug.LogError("No diseases available!");
            return;
        }

        assignedDisease = diseaseManager.diseaseDataList[Random.Range(0, diseaseManager.diseaseDataList.Count)];

        patientSymptoms.Clear();
        int numSymptoms = Random.Range(1, Mathf.Min(3, assignedDisease.symptoms.Count + 1));

        while (patientSymptoms.Count < numSymptoms)
        {
            string symptom = assignedDisease.symptoms[Random.Range(0, assignedDisease.symptoms.Count)];
            if (!patientSymptoms.Contains(symptom))
                patientSymptoms.Add(symptom);
        }
    }
    public bool IsTestPositive(string testName)
    {
        if (diagnosticsManager == null)
        {
            Debug.LogError("DiagnosticsManager is not assigned!");
            return false;
        }

        if (assignedDisease == null)
        {
            Debug.LogError("No disease assigned to patient!");
            return false;
        }

        foreach (var test in diagnosticsManager.GetActiveTests())
        {
            if (test.testName == testName && diagnosticsManager.IsOverDropZone(test))
            {
                return assignedDisease.tests.Contains(testName);
            }
        }

        return false;
    }


    /* -------------------- UI Population -------------------- */
    private void PopulateDropdowns()
    {
        diseaseChoices.ClearOptions();
        treatmentChoices.ClearOptions();

        if (diseaseManager == null)
        {
            Debug.LogError("DiseaseManager not found!");
            return;
        }

        List<string> diseaseNames = new List<string>();
        foreach (var disease in diseaseManager.diseaseDataList)
        {
            diseaseNames.Add(disease.diseaseName);
        }
        diseaseChoices.AddOptions(diseaseNames);

        treatmentChoices.options.Add(new TMP_Dropdown.OptionData("Emergency Room"));
        treatmentChoices.options.Add(new TMP_Dropdown.OptionData("Medicine"));
        treatmentChoices.options.Add(new TMP_Dropdown.OptionData("Surgery"));

        diseaseChoices.value = 0;
        treatmentChoices.value = 0;

        diseaseChoices.RefreshShownValue();
        treatmentChoices.RefreshShownValue();

        selectedDisease = diseaseChoices.options[0].text;
        selectedTreatment = treatmentChoices.options[0].text;
    }

    /* -------------------- Diagnosis Verification -------------------- */
    public void CheckDiagnosis()
    {
        if (currentPatient == null || assignedDisease == null)
        {
            Debug.LogError("No patient or disease data available!");
            return;
        }

        bool correctDisease = selectedDisease == assignedDisease.diseaseName;
        bool correctTreatment = assignedDisease.treatments.Contains(selectedTreatment);

        ProcessDiagnosis(correctDisease, correctTreatment);
        SpawnNextPatient();
    }

    private void ProcessDiagnosis(bool correctDiagnosis, bool correctTreatment)
    {
        if (PlayerStats.Instance == null)
        {
            Debug.LogError("PlayerStats not found!");
            return;
        }

        PlayerStats.Instance.totalPatients++;

        string penaltyMessage = "Daily Penalties: " + PlayerStats.Instance.dailyPenalties +
                                " | Weekly Penalties: " + PlayerStats.Instance.weeklyPenalties;

        if (correctDiagnosis && correctTreatment)
        {
            PlayerStats.Instance.patientsCured++;
            PlayerStats.Instance.totalEarnings += 500;
        }
        else
        {
            if (!correctDiagnosis)
            {
                Debug.Log("Incorrect Disease. Moving to next patient...");
                PlayerStats.Instance.AddPenalty(50);
                penaltyMessage = "Diagnosed wrong, penalty added. " + penaltyMessage;
            }
            if (!correctTreatment)
            {
                Debug.Log("Incorrect Treatment. Moving to next patient...");
                PlayerStats.Instance.AddPenalty(50);
                penaltyMessage = "Treatment wrong, penalty added. " + penaltyMessage;
            }
        }

        UpdatePenaltyUI();

        SpawnNextPatient();
    }

    /* -------------------- UI Event Handlers -------------------- */
    private void OnDropdownChanged()
    {
        selectedDisease = diseaseChoices.options[diseaseChoices.value].text;
        selectedTreatment = treatmentChoices.options[treatmentChoices.value].text;
    }

    /* -------------------- Data Retrieval -------------------- */
    public DiseaseData GetAssignedDisease() => assignedDisease;

    /* -------------------- Update Penalty UI -------------------- */
    private void UpdatePenaltyUI()
    {
        if (dailyPenaltyText != null)
        {
            dailyPenaltyText.text = "Daily Penalties: " + PlayerStats.Instance.dailyPenalties;
        }

        if (totalPenaltyText != null)
        {
            totalPenaltyText.text = "Total Penalties: " + PlayerStats.Instance.weeklyPenalties;
        }
    }
}
