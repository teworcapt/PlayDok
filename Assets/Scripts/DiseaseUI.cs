using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class DiseaseUI : MonoBehaviour
{
    public GameObject diseasePrefab;
    public Transform contentPanel;
    public List<DiseaseInfo> diseases;

    void Start()
    {
        PopulateDiseases();
    }

    void PopulateDiseases()
    {
        foreach (DiseaseInfo disease in diseases)
        {
            GameObject newPanel = Instantiate(diseasePrefab, contentPanel);
            newPanel.transform.Find("name").GetComponent<TMP_Text>().text = disease.diseaseName;
            newPanel.transform.Find("symptoms").GetComponent<TMP_Text>().text = disease.symptoms;
            newPanel.transform.Find("tests").GetComponent<TMP_Text>().text = string.Join(", ", disease.tests);
        }
    }
}
