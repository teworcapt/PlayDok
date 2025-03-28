using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class RulebookManager : MonoBehaviour
{
    [Header("Monitor & Rulebook UI")]
    public GameObject monitorPanel;
    public GameObject rulebookUI;
    public Button monitorButton;
    public Transform diseaseList;
    public GameObject diseasePrefab;

    [Header("Disease Data")]
    public List<DiseaseData> diseases;

    private bool isMonitorOpen = false;

    /* -------------------- Initialization -------------------- */

    private void Start()
    {
        monitorButton.onClick.AddListener(ToggleMonitor);
        monitorPanel.SetActive(false);
        rulebookUI.SetActive(false);
        PopulateDiseases();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Spacebar pressed!");
            ToggleMonitor(!isMonitorOpen);
        }

        if (isMonitorOpen && Input.GetMouseButtonDown(0))
        {
            if (!IsPointerOverUIObject())
            {
                ToggleMonitor(false);
            }
        }
    }

    /* -------------------- Toggle Monitor & Rulebook -------------------- */

    public void ToggleMonitor()
    {
        isMonitorOpen = !isMonitorOpen;
        monitorPanel.SetActive(isMonitorOpen);
        rulebookUI.SetActive(isMonitorOpen);
    }

    public void ToggleMonitor(bool state)
    {
        isMonitorOpen = state;
        monitorPanel.SetActive(state);
        rulebookUI.SetActive(state);

        Debug.Log($"Monitor toggled: {state}");     
    }

    /* -------------------- Disease List Population -------------------- */

    private void PopulateDiseases()
    {
        if (diseaseList == null || diseasePrefab == null) return;

        foreach (DiseaseData disease in diseases)
        {
            if (disease == null) continue;

            GameObject diseaseRow = Instantiate(diseasePrefab, diseaseList);
            TextMeshProUGUI[] textElements = diseaseRow.GetComponentsInChildren<TextMeshProUGUI>();

            if (textElements.Length >= 4)
            {
                textElements[0].text = disease.diseaseName;
                textElements[1].text = string.Join(", ", disease.symptoms);
                textElements[2].text = string.Join(", ", disease.tests);
                textElements[3].text = string.Join(", ", disease.treatments);
            }
        }
    }

    private bool IsPointerOverUIObject()
    {
        return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
    }
}
