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
                return disease; // Return the matching disease info
            }
        }
        return null;
    }
}
