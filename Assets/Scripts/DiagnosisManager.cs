using UnityEngine;

public class DiagnosisManager : MonoBehaviour
{
    public static DiagnosisManager Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    public void CheckDiagnosis(PatientData patient, string selectedDisease)
    {
        if (patient == null) return;

        bool isCorrect = (patient.disease == selectedDisease);

        if (isCorrect)
        {
            Debug.Log("Correct Diagnosis!");
        }
        else
        {
            Debug.Log("Incorrect Diagnosis!");
        }
    }
}
