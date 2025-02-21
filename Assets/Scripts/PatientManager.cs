using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PatientManager : MonoBehaviour
{
    public PatientData[] patients;
    public GameObject patientPanelPrefab;
    public Transform panelParent;
    public Image patientImage;

    private GameObject currentPanel;

    void Start()
    {
        SpawnNextPatient();
    }

    public void SpawnNextPatient()
    {
        if (currentPanel != null) Destroy(currentPanel);

        PatientData randomPatient = patients[Random.Range(0, patients.Length)];

        // Instantiate and set up the panel
        currentPanel = Instantiate(patientPanelPrefab, panelParent);
        currentPanel.GetComponent<PatientPanel>().Setup(randomPatient);

        // Update the patient sprite
        patientImage.sprite = randomPatient.patientSprite;
    }
}
