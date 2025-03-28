using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class DiseaseManager : MonoBehaviour
{
    public static DiseaseManager Instance { get; private set; }

    [Header("Disease Database")]
    public List<DiseaseData> diseaseDataList;

    private Dictionary<string, DiseaseData> diseaseDictionary;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeDiseaseDictionary();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeDiseaseDictionary()
    {
        diseaseDictionary = diseaseDataList.ToDictionary(disease => disease.diseaseName);
    }

    public DiseaseData GetDiseaseInfo(string diseaseName)
    {
        diseaseDictionary.TryGetValue(diseaseName, out DiseaseData disease);
        return disease;
    }

    public List<string> GetTreatments(string diseaseName)
    {
        return GetDiseaseInfo(diseaseName)?.treatments ?? new List<string>();
    }
}
