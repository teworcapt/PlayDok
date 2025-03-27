using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class DiagnosisManager : MonoBehaviour
{
    public static DiagnosisManager Instance { get; private set; }

    public TMP_Dropdown diagnosisDropdown;
    public TMP_Dropdown treatmentDropdown;

    private string correctDiagnosis;
    private string correctTreatment;

    private Dictionary<string, bool> testResults = new Dictionary<string, bool>();

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

    public void AssignRandomDiagnosis(string diseaseName)
    {
        correctDiagnosis = diseaseName;

        DiseaseData diseaseData = DiseaseManager.Instance?.GetDiseaseData(diseaseName);
        if (diseaseData != null && diseaseData.treatments.Count > 0)
        {
            correctTreatment = diseaseData.treatments[0];
        }
        else
        {
            correctTreatment = "Unknown"; // Default if no treatment found
        }

        Debug.Log($"[DiagnosisManager] Correct diagnosis: {correctDiagnosis}, Correct treatment: {correctTreatment}");
    }

    public string GetCurrentDisease()
    {
        return correctDiagnosis;
    }

    public void SubmitDiagnosis()
    {
        string selectedDiagnosis = diagnosisDropdown.options[diagnosisDropdown.value].text;
        string selectedTreatment = treatmentDropdown.options[treatmentDropdown.value].text;

        bool diagnosisCorrect = (selectedDiagnosis == correctDiagnosis);
        bool treatmentCorrect = (selectedTreatment == correctTreatment);

        if (diagnosisCorrect && treatmentCorrect)
        {
            Debug.Log("[DiagnosisManager] Diagnosis and treatment are both correct!");
        }
        else if (!diagnosisCorrect && treatmentCorrect)
        {
            Debug.Log("[DiagnosisManager] Incorrect diagnosis but correct treatment.");
        }
        else if (diagnosisCorrect && !treatmentCorrect)
        {
            Debug.Log("[DiagnosisManager] Correct diagnosis but incorrect treatment.");
        }
        else
        {
            Debug.Log("[DiagnosisManager] Both diagnosis and treatment are incorrect.");
        }
    }
}
