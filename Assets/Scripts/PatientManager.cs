using UnityEngine;
using UnityEngine.UI;

public class PatientManager : MonoBehaviour
{
    public PatientData[] patients;
    public Image patientImage;
    public PatientDropzone patientDropzone;
    public DiagnosticsManager[] diagnosticsManagers;

    void Start()
    {
        SpawnNextPatient();
    }

    public void SpawnNextPatient()
    {
        if (patients == null || patients.Length == 0)
        {
            Debug.LogError("Patients array is NULL!");
            return;
        }

        Debug.Log("Patients available: " + patients.Length);

        PatientData randomPatient = patients[Random.Range(0, patients.Length)];

        if (diagnosticsManagers != null && diagnosticsManagers.Length > 0)
        {
            foreach (DiagnosticsManager dm in diagnosticsManagers)
            {
                dm.ResetDiagnostics();
            }
        }
        else
        {
            Debug.LogError("No DiagnosticsManager references found!");
        }

        if (patientDropzone != null)
        {
            Debug.Log("Assigning patient: " + randomPatient.patientName);
            patientDropzone.SetPatient(randomPatient);
        }
        else
        {
            Debug.LogError("PatientDropzone reference is missing in PatientManager!");
        }

        if (patientImage != null)
        {
            patientImage.sprite = randomPatient.patientSprite;
        }
    }
}
