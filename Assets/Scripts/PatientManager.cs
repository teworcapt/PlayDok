using UnityEngine;
using UnityEngine.UI;
<<<<<<< Updated upstream

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
=======
using System.Collections.Generic;
using TMPro;

public class PatientManager : MonoBehaviour
{
    public static PatientManager Instance;

    public GameObject patient;
    public Image patientImage;
    public TextMeshProUGUI patientNameText;

    [SerializeField] private List<PatientData> availablePatients = new List<PatientData>();
    private PatientData currentPatient;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        LoadPatients();
        AssignRandomPatient();
    }

    private void LoadPatients()
>>>>>>> Stashed changes
    {
        PatientData[] patients = Resources.LoadAll<PatientData>("Patients");

        foreach (PatientData patient in patients)
        {
<<<<<<< Updated upstream
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
=======
            if (!availablePatients.Contains(patient))
            {
                availablePatients.Add(patient);
            }
        }
    }

    public List<PatientData> GetAvailablePatients()
    {
        return availablePatients;
    }

    public void AssignPatient(PatientData newPatient)
    {
        currentPatient = newPatient;
        UpdateUI();
    }

    public void AssignRandomPatient()
    {
        if (availablePatients.Count == 0)
        {
            Debug.LogWarning("No available patients!");
            return;
        }

        int randomIndex = Random.Range(0, availablePatients.Count);
        currentPatient = availablePatients[randomIndex];

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (currentPatient != null)
        {
            patientNameText.text = currentPatient.patientName;
            patientImage.sprite = currentPatient.patientSprite;
>>>>>>> Stashed changes
        }
    }
}
