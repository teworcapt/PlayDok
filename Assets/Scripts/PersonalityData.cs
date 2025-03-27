using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewPersonality", menuName = "Scriptable Objects/PersonalityData")]
public class PersonalityData : ScriptableObject
{
    public string personalityName;
    public Sprite defaultSprite;
    public List<string> dialogues;
}
