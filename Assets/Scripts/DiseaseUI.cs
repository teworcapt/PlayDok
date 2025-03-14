using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class DiseaseUI : MonoBehaviour
{
    public Transform diseaseList;
    public List<DiseaseInfo> diseases;
    public GameObject headerPrefab;
    public TMP_FontAsset headerFont;
    private bool headerCreated = false;

    void Start()
    {
        PopulateDiseases();
    }

    void PopulateDiseases()
    {
        if (diseaseList == null) return;

        if (!headerCreated && headerPrefab != null)
        {
            GameObject header = Instantiate(headerPrefab, diseaseList);
            header.transform.SetAsFirstSibling();
            headerCreated = true;
        }

        foreach (DiseaseInfo disease in diseases)
        {
            if (disease == null) continue;

            GameObject diseaseRow = new GameObject("DiseaseRow", typeof(RectTransform));
            diseaseRow.transform.SetParent(diseaseList, false);

            HorizontalLayoutGroup layout = diseaseRow.AddComponent<HorizontalLayoutGroup>();
            layout.childControlWidth = false;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.spacing = 20;

            AddTextElement(diseaseRow.transform, disease.diseaseName, 200);
            AddTextElement(diseaseRow.transform, string.Join(", ", disease.symptoms), 300);
            AddTextElement(diseaseRow.transform, string.Join(", ", disease.tests), 300);
            AddTextElement(diseaseRow.transform, string.Join(", ", disease.treatments), 300);
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
    }
}
