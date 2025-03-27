using UnityEngine;
using System.Collections.Generic;

public class DiseaseManager : MonoBehaviour
{
<<<<<<< Updated upstream
    public List<DiseaseInfo> allDiseases = new List<DiseaseInfo>(); // Store all diseases
=======
    public static DiseaseManager Instance { get; private set; }
>>>>>>> Stashed changes

    [SerializeField] private List<DiseaseData> allDiseases = new List<DiseaseData>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

<<<<<<< Updated upstream

    public DiseaseInfo GetDiseaseInfo(string diseaseName)
=======
    public DiseaseData GetDiseaseData(string diseaseName)
>>>>>>> Stashed changes
    {
        if (string.IsNullOrEmpty(diseaseName))
        {
            Debug.LogError("[DiseaseManager] Attempted to get disease data with an empty or null name.");
            return null;
        }

        DiseaseData disease = allDiseases.Find(d => d.diseaseName == diseaseName);

        if (disease == null)
        {
            Debug.LogError($"[DiseaseManager] Disease '{diseaseName}' not found in the database.");
        }

        return disease;
    }
<<<<<<< Updated upstream
=======

    public List<string> GetTreatments(string diseaseName)
    {
        DiseaseData disease = GetDiseaseData(diseaseName);
        return disease != null ? new List<string>(disease.treatments) : new List<string>();
    }

    public List<string> GetTests(string diseaseName)
    {
        DiseaseData disease = GetDiseaseData(diseaseName);
        return disease != null ? new List<string>(disease.tests) : new List<string>();
    }

    public bool DoesDiseaseRequireTest(string diseaseName, string testName)
    {
        DiseaseData disease = GetDiseaseData(diseaseName);
        return disease != null && disease.tests.Contains(testName);
    }
>>>>>>> Stashed changes
}
