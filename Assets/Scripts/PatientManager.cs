using UnityEngine;
using UnityEngine.UI;

public class PatientManager : MonoBehaviour
{
    public PatientData[] patients;
    public Image patientImage;
    public PatientDropzone patientDropzone; // Updated to use PatientDropzone

    void Start()
    {
        SpawnNextPatient();
    }
    public void SpawnNextPatient()
    {
        if (patients == null || patients.Length == 0)
        {
            Debug.LogError("❌ Patients array is EMPTY or NULL! Assign PatientData in the Inspector.");
            return;
        }

        Debug.Log("✅ Patients available: " + patients.Length);

        PatientData randomPatient = patients[Random.Range(0, patients.Length)];

        if (patientDropzone != null)
        {
            Debug.Log("✅ Assigning patient: " + randomPatient.patientName);
            patientDropzone.SetPatient(randomPatient);
        }
        else
        {
            Debug.LogError("❌ PatientDropzone reference is missing in PatientManager!");
        }

        if (patientImage != null)
        {
            patientImage.sprite = randomPatient.patientSprite;
        }
    }


}
