using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class DiagnosisManager : MonoBehaviour
{
    public static DiagnosisManager Instance { get; private set; }

    public DiseaseManager diseaseManager;
    public PatientManager patientManager;
    public DiagnosticsManager diagnosticsManager;

    public List<PatientData> availablePersonalities;

    private PatientData currentPatient;
    private DiseaseData assignedDisease;
    private List<string> patientSymptoms = new List<string>();

    public TMP_Dropdown diseaseChoices;
    public TMP_Dropdown treatmentChoices;
    public TextMeshProUGUI symptomsText;
    public Button submitButton;

    private string selectedDisease;
    private string selectedTreatment;

    private Color redColor;
    private Color greenColor;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        if (!ColorUtility.TryParseHtmlString("#ff3629", out redColor))
            Debug.LogError("DiagnosisManager: Failed to parse red hex color.");

        if (!ColorUtility.TryParseHtmlString("#a1cd3a", out greenColor))
            Debug.LogError("DiagnosisManager: Failed to parse green hex color.");
    }

    private void Start()
    {
        diseaseChoices.onValueChanged.AddListener(delegate { OnDropdownChanged(); });
        treatmentChoices.onValueChanged.AddListener(delegate { OnDropdownChanged(); });
        submitButton.onClick.AddListener(CheckDiagnosis);
        SpawnNextPatient();
    }

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

    public void CheckDiagnosis()
    {
        if (!DiagnosticsManager.Instance.CanMoveToNextPatient)
        {
            StartCoroutine(patientManager.ShowNoTestSubmitDialogCoroutine());
            return;
        }

        if (currentPatient == null || assignedDisease == null)
        {
            Debug.LogError("No patient or disease data available!");
            return;
        }

        bool correctDisease = selectedDisease == assignedDisease.diseaseName;
        bool correctTreatment = assignedDisease.treatments.Contains(selectedTreatment);

        ProcessDiagnosis(correctDisease, correctTreatment);
    }

    private void ProcessDiagnosis(bool correctDiagnosis, bool correctTreatment)
    {
        if (PlayerStats.Instance == null)
        {
            Debug.LogError("PlayerStats not found!");
            return;
        }

        PlayerStats.Instance.totalPatients++;

        if (correctDiagnosis && correctTreatment)
        {
            PlayerStats.Instance.patientsCured++;
            PlayerStats.Instance.totalEarnings += 500;
            NotificationManager.Instance?.ShowNotification("Correct Diagnosis!", greenColor, NotificationType.CorrectDiagnosis);
        }
        else
        {
            if (!correctDiagnosis)
            {
                PlayerStats.Instance.totalEarnings += 250;
                PlayerStats.Instance.AddPenalty(50);

            }
            if (!correctTreatment)
            {
                PlayerStats.Instance.totalEarnings += 250;
                PlayerStats.Instance.AddPenalty(50);
            }

            NotificationManager.Instance?.ShowNotification("Incorrect Diagnosis or Treatment!", redColor, NotificationType.IncorrectDiagnosis);
        }

        if (TimerManager.Instance.GetRemainingDayTime() <= 0)
        {
            TimerManager.Instance.FinalDiagnosisComplete();
        }
        else
        {
            SpawnNextPatient();
            ProgressManager.Instance.PatientCured(correctDiagnosis, correctTreatment);
        }
    }

    private void OnDropdownChanged()
    {
        selectedDisease = diseaseChoices.options[diseaseChoices.value].text;
        selectedTreatment = treatmentChoices.options[treatmentChoices.value].text;
    }

    public DiseaseData GetAssignedDisease() => assignedDisease;
}