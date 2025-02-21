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
        PatientData randomPatient = patients[Random.Range(0, patients.Length)];

        if (patientDropzone != null)
        {
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
