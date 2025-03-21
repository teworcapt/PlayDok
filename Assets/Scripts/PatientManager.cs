using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class PatientManager : MonoBehaviour
{
    public static PatientManager Instance { get; private set; }

    [Header("Patient List")]
    public PatientData[] patients;

    [Header("Patient UI")]
    public Image patientImage;
    public TextMeshProUGUI patientNameHolder;
    public TextMeshProUGUI symptomsText;
    public TMP_Dropdown diseaseChoices;
    public TMP_Dropdown treatmentChoices;
    public Button submitButton;

    private PatientData currentPatient;
    private List<string> validTests = new List<string>();
    private string selectedDisease;
    private string selectedTreatment;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        PopulateDropdowns();
        diseaseChoices.onValueChanged.AddListener(delegate { OnDiseaseChanged(); });
        treatmentChoices.onValueChanged.AddListener(delegate { OnTreatmentChanged(); });
        submitButton.onClick.AddListener(CheckDiagnosis);
        SpawnNextPatient();
    }

    private void SpawnNextPatient()
    {
        if (patients == null || patients.Length == 0)
        {
            Debug.LogError("No patients available!");
            return;
        }

        currentPatient = patients[Random.Range(0, patients.Length)];
        DiseaseInfo diseaseInfo = DiseaseManager.Instance.allDiseases.Find(d => d.diseaseName == currentPatient.disease);

        if (diseaseInfo != null && diseaseInfo.symptoms.Count > 0)
        {
            List<string> shuffledSymptoms = new List<string>(diseaseInfo.symptoms);
            ShuffleList(shuffledSymptoms);

            int symptomCount = Random.Range(1, 3);
            currentPatient.symptoms = shuffledSymptoms.GetRange(0, Mathf.Min(symptomCount, shuffledSymptoms.Count));
        }
        else
        {
            Debug.LogError($"No symptoms found for {currentPatient.disease}!");
            currentPatient.symptoms.Clear();
        }

        validTests = new List<string>(currentPatient.tests);

        DiagnosticsManager.Instance?.ResetDiagnostics();

        patientImage.sprite = currentPatient.patientSprite;
        patientNameHolder.text = $"Patient Name: {currentPatient.patientName}";
        symptomsText.text = $"Symptoms: {(currentPatient.symptoms.Count > 0 ? string.Join(", ", currentPatient.symptoms) : "No symptoms")}";

        PopulateDropdowns();
        TimerManager.Instance.StartPatientProcessing();
    }

    private void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
        }
    }

    private void CheckDiagnosis()
    {
        if (currentPatient == null)
        {
            Debug.LogError("No patient data available!");
            return;
        }

        bool correctDisease = selectedDisease == currentPatient.disease;
        bool correctTreatment = DiseaseManager.Instance.allDiseases.Find(d => d.diseaseName == selectedDisease)?.treatments.Contains(selectedTreatment) ?? false;

        ProcessDiagnosis(correctDisease, correctTreatment);

        TimerManager.Instance.CompletePatientProcessing();
        SpawnNextPatient();
    }

    private void PopulateDropdowns()
    {
        diseaseChoices.ClearOptions();
        treatmentChoices.ClearOptions();

        if (DiseaseManager.Instance == null)
        {
            Debug.LogError("DiseaseManager not found!");
            return;
        }

        foreach (DiseaseInfo disease in DiseaseManager.Instance.allDiseases)
        {
            diseaseChoices.options.Add(new TMP_Dropdown.OptionData(disease.diseaseName));
        }

        treatmentChoices.options.Add(new TMP_Dropdown.OptionData("Emergency Room"));
        treatmentChoices.options.Add(new TMP_Dropdown.OptionData("Medicine"));
        treatmentChoices.options.Add(new TMP_Dropdown.OptionData("Surgery"));

        diseaseChoices.value = 0;
        diseaseChoices.RefreshShownValue();
        selectedDisease = diseaseChoices.options[0].text;

        treatmentChoices.value = 0;
        treatmentChoices.RefreshShownValue();
        selectedTreatment = treatmentChoices.options[0].text;
    }

    public void ProcessDiagnosis(bool correctDiagnosis, bool correctTreatment)
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
        }
        else
        {
            PlayerStats.Instance.AddPenalty(50);
        }
    }

    private void OnDiseaseChanged()
    {
        selectedDisease = diseaseChoices.options[diseaseChoices.value].text;
    }

    private void OnTreatmentChanged()
    {
        selectedTreatment = treatmentChoices.options[treatmentChoices.value].text;
    }

    public bool IsTestPositive(string testName)
    {
        return validTests.Contains(testName);
    }
}
