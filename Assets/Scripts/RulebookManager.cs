using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class RulebookManager : MonoBehaviour
{
    public Transform diseaseList;
    public List<DiseaseInfo> diseases;
    public GameObject diseasePrefab;
    public TMP_FontAsset headerFont;

    void Start()
    {
        PopulateDiseases();
    }

    void PopulateDiseases()
    {
        if (diseaseList == null || diseasePrefab == null) return;

        foreach (DiseaseInfo disease in diseases)
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

    void AddTextElement(Transform parent, string content, float width)
    {
        GameObject textObj = new GameObject("Text", typeof(RectTransform));
        textObj.transform.SetParent(parent, false);

        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = string.IsNullOrEmpty(content) ? "N/A" : content;
        text.fontSize = 20;
        text.alignment = TextAlignmentOptions.Left;
        text.enableAutoSizing = false;

        if (headerFont != null)
        {
            text.font = headerFont;
        }

        LayoutElement layoutElement = textObj.AddComponent<LayoutElement>();
        layoutElement.minHeight = 30;
        layoutElement.preferredWidth = width;
        layoutElement.flexibleWidth = 0;
    }
}
