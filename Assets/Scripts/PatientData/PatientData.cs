using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "PatientData", menuName = "Scriptable Objects/Patient Data")]
public class PatientData : ScriptableObject
{
    public enum PersonalityCategory { Positive, Neutral, Negative }
    public enum PersonalitySubcategory
    {
        Peppy, Lively, Jock,  // Positive
        Normal, Polite, Mature, // Neutral
        Cranky, Snooty, SociallyAwkward // Negative
    }

    [Header("Patient Info")]
    public string patientName;
    public PersonalityCategory personalityCategory;
    public PersonalitySubcategory personalitySubcategory;

    [Header("Random Name Pool")]
    public List<string> availableNames = new List<string>();

    [Header("Dialogues")]
    public List<DialogueSet> dialogues = new List<DialogueSet>();

    [Header("No Test Submit Dialogue")]
    public List<string> noTestSubmitLines = new List<string>();

    [Header("Patient Appearance")]
    public List<Sprite> patientSprites = new List<Sprite>();
    public Sprite selectedSprite;

    [Header("Gameplay Settings")]
    public float baseTimePenalty = 5f;

    [Header("Disease Info")]
    public DiseaseData diseaseData;

    [System.Serializable]
    public class DialogueSet
    {
        public string doctorQuestion;
        public string patientReply;
        public string doctorResponsePositive;
        public string patientReactionPositive;
        public string doctorResponseNegative;
        public string patientReactionNegative;
    }

    public void Initialize()
    {
        if (availableNames.Count > 0)
        {
            patientName = availableNames[Random.Range(0, availableNames.Count)];
        }

        if (patientSprites.Count > 0)
        {
            selectedSprite = patientSprites[Random.Range(0, patientSprites.Count)];
        }
        else
        {
            Debug.LogWarning($"Patient '{patientName}' has no sprites assigned.");
        }

        AssignBaseTimePenalty();
    }

    private void AssignBaseTimePenalty()
    {
        switch (personalityCategory)
        {
            case PersonalityCategory.Positive:
                baseTimePenalty = 2f;
                break;
            case PersonalityCategory.Neutral:
                baseTimePenalty = 5f;
                break;
            case PersonalityCategory.Negative:
                baseTimePenalty = 8f;
                break;
        }
    }

    public float GetTimePenalty(bool positiveResponse)
    {
        float penaltyPercentage = 0f;
        float bonusPercentage = 0f;

        int dailyMistakes = PlayerStats.Instance.dailyPenalties;

        switch (personalityCategory)
        {
            case PersonalityCategory.Positive:
                if (positiveResponse)
                {
                    bonusPercentage = 0.05f;
                }
                break;

            case PersonalityCategory.Neutral:
                if (positiveResponse)
                {
                    bonusPercentage = 0.05f;
                }
                else
                {
                    penaltyPercentage = 0.05f;
                }
                break;

            case PersonalityCategory.Negative:
                if (positiveResponse)
                {
                    bonusPercentage = 0.08f;
                }
                else
                {
                    penaltyPercentage = 0.10f;
                }
                break;
        }

        penaltyPercentage += dailyMistakes * 0.02f;

        float timeAdjustment = baseTimePenalty * (penaltyPercentage - bonusPercentage);

        return baseTimePenalty + timeAdjustment;
    }

}
