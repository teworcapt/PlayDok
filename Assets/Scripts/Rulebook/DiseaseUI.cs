using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class DiseaseUI : MonoBehaviour
{
    public GameObject diseasePrefab;
    public Transform contentPanel;

    [SerializeField]
    public List<DiseaseInfo> diseases = new List<DiseaseInfo>();

    void Start()
    {
        Debug.Log("Disease list count: " + diseases.Count);
        PopulateDiseases();
    }

    void PopulateDiseases()
    {
        foreach (DiseaseInfo disease in diseases)
        {
            GameObject newPanel = Instantiate(diseasePrefab, contentPanel);

            newPanel.transform.Find("name").GetComponent<TMP_Text>().text = disease.name;
            newPanel.transform.Find("symptoms").GetComponent<TMP_Text>().text = disease.symptoms;
            newPanel.transform.Find("tests").GetComponent<TMP_Text>().text = disease.tests;
            newPanel.transform.Find("treatment").GetComponent<TMP_Text>().text = disease.treatment;

            newPanel.SetActive(true);
        }
    }
}
