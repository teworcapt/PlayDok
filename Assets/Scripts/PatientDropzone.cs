using UnityEngine;
using TMPro;

public class PatientDropzone : MonoBehaviour
{
    private PatientData currentPatient;

    public TextMeshProUGUI patientName;
    public TextMeshProUGUI symptoms;

    public void SetPatient(PatientData patient)
    {
        currentPatient = patient;

        if (patientName != null)
            patientName.text = patient.patientName;

        if (symptoms != null)
            symptoms.text = string.Join(", ", patient.symptoms);
    }

    public PatientData GetCurrentPatient()
    {
        return currentPatient;
    }
}
