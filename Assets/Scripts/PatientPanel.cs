using UnityEngine;
using TMPro;

public class PatientPanel : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI symptomsText;

    public void Setup(PatientData data)
    {
        nameText.text = "Name: " + data.patientName;
        symptomsText.text = "Symptoms: " + string.Join(", ", data.symptoms);
    }
}
