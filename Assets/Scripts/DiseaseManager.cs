using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "DiseaseManager", menuName = "Scriptable Objects/Disease Manager")]
public class DiseaseManager : ScriptableObject
{
    public List<DiseaseInfo> allDiseases = new List<DiseaseInfo>(); // Store all diseases

    private static DiseaseManager _instance;

    public static DiseaseManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<DiseaseManager>("DiseaseManager");
                if (_instance == null)
                {
                    Debug.LogError("DiseaseManager asset not found in Resources folder!");
                }
            }
            return _instance;
        }
    }


    public DiseaseInfo GetDiseaseInfo(string diseaseName)
    {
        foreach (DiseaseInfo disease in allDiseases)
        {
            if (disease.diseaseName == diseaseName)
            {
                return disease;
            }
        }
        return null;
    }
}
