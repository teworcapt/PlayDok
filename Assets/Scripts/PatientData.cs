using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "PatientData", menuName = "Scriptable Objects/PatientData")]
public class PatientData : ScriptableObject
{
    public string patientName;
    public string disease;
    public List<string> symptoms = new List<string>();
    public List<string> tests = new List<string>();    
    public List<string> treatment = new List<string>(); 
    public Sprite patientSprite;
}
