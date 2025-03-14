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
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        PopulateDropdowns();
        diseaseChoices.onValueChanged.AddListener(delegate { OnDiseaseChanged(); });
        treatmentChoices.onValueChanged.AddListener(delegate { OnTreatmentChanged(); });
        submitButton.onClick.AddListener(CheckDiagnosis);
        SpawnNextPatient();
    }

    public void SpawnNextPatient()
    {
        if (patients == null || patients.Length == 0)
        {
            Debug.LogError("Patients array is NULL!");
            return;
        }

        currentPatient = patients[Random.Range(0, patients.Length)];
        validTests = new List<string>(currentPatient.tests);

        if (DiagnosticsManager.Instance != null)
        {
            DiagnosticsManager.Instance.ResetDiagnostics();
        }
        else
        {
            Debug.LogError("DiagnosticsManager instance not found!");
        }

        if (patientImage != null)
        {
            patientImage.sprite = currentPatient.patientSprite;
        }

        if (patientNameHolder != null)
        {
            patientNameHolder.text = "Patient Name: " + currentPatient.patientName;
        }

        if (symptomsText != null)
        {
            symptomsText.text = "Symptoms: " + (currentPatient.symptoms.Count > 0
                ? string.Join(", ", currentPatient.symptoms)
                : "No symptoms");
        }

        PopulateDropdowns();
    }

    private void PopulateDropdowns()
    {
        if (DiseaseManager.Instance != null)
        {
            diseaseChoices.ClearOptions();
            treatmentChoices.ClearOptions();

            foreach (DiseaseInfo disease in DiseaseManager.Instance.allDiseases)
            {
                diseaseChoices.options.Add(new TMP_Dropdown.OptionData(disease.diseaseName));
            }

            treatmentChoices.options.Add(new TMP_Dropdown.OptionData("Emergency"));
            treatmentChoices.options.Add(new TMP_Dropdown.OptionData("Medicine"));

            if (diseaseChoices.options.Count > 0)
            {
                diseaseChoices.value = 0;
                selectedDisease = diseaseChoices.options[0].text;
            }

            if (treatmentChoices.options.Count > 0)
            {
                treatmentChoices.value = 0;
                selectedTreatment = treatmentChoices.options[0].text;
            }
        }
        else
        {
            Debug.LogError("DiseaseManager instance not found!");
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

    private void CheckDiagnosis()
    {
        if (currentPatient == null)
        {
            Debug.LogError("No patient data available!");
            return;
        }

        bool correctDisease = selectedDisease == currentPatient.disease;
        bool correctTreatment = selectedTreatment == "Emergency" || selectedTreatment == "Medicine";

        if (correctDisease && correctTreatment)
        {
            Debug.Log("Correct Diagnosis & Treatment. Moving to next patient...");
        }
        else if (!correctDisease)
        {
            Debug.Log("Incorrect Disease. Moving to next patient...");
        }
        else if (!correctTreatment)
        {
            Debug.Log("Incorrect Treatment. Moving to next patient...");
        }

        SpawnNextPatient();
    }

    public bool IsTestPositive(string testName)
    {
        return validTests.Contains(testName);
    }
}
