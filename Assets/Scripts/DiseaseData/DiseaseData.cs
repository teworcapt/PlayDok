using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewDisease", menuName = "Scriptable Objects/Disease Info")]
public class DiseaseInfo : ScriptableObject
{
    public string diseaseName;
    public List<string> symptoms = new List<string>();
    public List<string> tests = new List<string>();
    public List<string> treatments = new List<string>();
}
