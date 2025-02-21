using UnityEngine;

[CreateAssetMenu(fileName = "PatientData", menuName = "Scriptable Objects/PatientData")]
public class PatientData : ScriptableObject
{
    public string patientName;
    public string disease;
    [TextArea] public string[] symptoms;
    [TextArea] public string[] tests;
    [TextArea] public string[] treatment;
    public Sprite patientSprite;
}
