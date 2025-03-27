using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewDisease", menuName = "Scriptable Objects/DiseaseInfo")]
public class DiseaseData : ScriptableObject
{
    public string diseaseName;
    [TextArea] public string symptoms;
    public List<string> tests = new List<string>();
    public List<string> treatments = new List<string>();
}
