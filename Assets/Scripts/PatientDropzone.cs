using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PatientDropzone : MonoBehaviour
{
    private PatientData currentPatient;

    public TextMeshProUGUI patientName;
    public TextMeshProUGUI symptoms;
    public TMP_Dropdown diseaseDropdown;
    public Button diagnoseButton;

    private string selectedDisease;

    private void Start()
    {
        PopulateDropdown();
        diseaseDropdown.onValueChanged.AddListener(delegate { OnDropdownChanged(); });
        diagnoseButton.onClick.AddListener(CheckDiagnosis);
    }

    public void SetPatient(PatientData patient)
    {
        if (patient == null)
        {
            Debug.LogError("SetPatient called with NULL patient!");
            return;
        }

        currentPatient = patient;

        if (patientName != null)
        {
            Debug.Log("Setting patient name: " + patient.patientName);
            patientName.text = patient.patientName;
        }

        if (symptoms != null)
        {
            if (patient.symptoms.Count > 0)
            {
                Debug.Log("Setting symptoms: " + string.Join(", ", patient.symptoms));
                symptoms.text = string.Join(", ", patient.symptoms);
            }
            else
            {
                Debug.Log("No symptoms available!");
                symptoms.text = "No symptoms";
            }

            symptoms.text = string.Join(", ", patient.symptoms);
        }
    }


    public PatientData GetCurrentPatient()
    {
        return currentPatient;
    }


    private void PopulateDropdown()
    {
        diseaseDropdown.ClearOptions();
        if (DiseaseManager.Instance != null)
        {
            foreach (DiseaseInfo disease in DiseaseManager.Instance.allDiseases)
            {
                diseaseDropdown.options.Add(new TMP_Dropdown.OptionData(disease.diseaseName));
            }
            diseaseDropdown.value = 0;
            selectedDisease = diseaseDropdown.options[0].text;
        }
        else
        {
            Debug.LogError("DiseaseManager instance not found!");
        }
    }

    private void OnDropdownChanged()
    {
        selectedDisease = diseaseDropdown.options[diseaseDropdown.value].text;
    }

    private void CheckDiagnosis()
    {
        if (currentPatient == null)
        {
            Debug.LogError("No patient data available!");
            return;
        }

        if (selectedDisease == currentPatient.disease)
        {
            Debug.Log("Correct Diagnosis! Moving to next patient...");
            FindFirstObjectByType<PatientManager>().SpawnNextPatient();
        }
        else
        {
            Debug.Log("Incorrect diagnosis. Try again!");
        }
    }
}
