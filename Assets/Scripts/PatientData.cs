using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewPatient", menuName = "Scriptable Objects/PatientData")]
public class PatientData : ScriptableObject
{
    public string patientName;
    public DiseaseData disease;
    public PersonalityData personality;
    public List<string> symptoms = new List<string>();
    public List<string> tests = new List<string>();
    public List<string> treatments = new List<string>();
    public Sprite patientSprite;
}
