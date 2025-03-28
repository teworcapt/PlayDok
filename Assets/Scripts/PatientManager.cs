using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PatientManager : MonoBehaviour
{
    public static PatientManager Instance { get; private set; }

    [Header("Patient UI")]
    public Image patientImage;
    public TextMeshProUGUI patientNameHolder;
    public TextMeshProUGUI dialogNameHolder;
    public TextMeshProUGUI symptomsText;
    public TextMeshProUGUI dialogueText;

    [Header("Interrogation UI")]
    public GameObject dialogBox;
    public Button interrogateButton;
    public Button continueButton;
    public Button choiceOneButton;
    public Button choiceTwoButton;
    public GameObject rulebook;

    private PatientData currentPatient;
    private List<string> currentSymptoms;
    private bool awaitingResponse = false;
    private bool hasAsked = false;

    private string doctorDialogue;
    private string patientDialogue;
    private string responsePositive;
    private string responseNegative;

    /* -------------------- Initialization -------------------- */
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        interrogateButton.onClick.AddListener(StartInterrogation);
        continueButton.onClick.AddListener(HandleContinue);
        choiceOneButton.onClick.AddListener(() => SelectResponse(true));
        choiceTwoButton.onClick.AddListener(() => SelectResponse(false));

        ResetUI();
    }

    /* -------------------- Patient Setup -------------------- */
    public void UpdatePatientUI(PatientData patient, List<string> symptoms)
    {
        if (patient == null || patient.patientSprites == null || patient.patientSprites.Count == 0)
        {
            Debug.LogError($"Invalid patient data for {patient?.patientName ?? "unknown"}!");
            return;
        }

        currentPatient = patient;
        currentSymptoms = new List<string>(symptoms);

        patientImage.sprite = patient.patientSprites[Random.Range(0, patient.patientSprites.Count)];
        patientNameHolder.text = patient.patientName;
        dialogNameHolder.text = patient.patientName;
        symptomsText.text = string.Join(", ", currentSymptoms);

        hasAsked = false;
        awaitingResponse = false;
        interrogateButton.interactable = true;
        interrogateButton.GetComponentInChildren<TextMeshProUGUI>().text = "Interrogate";
    }

    /* -------------------- Interrogation Flow -------------------- */
    private void StartInterrogation()
    {
        if (hasAsked) return;

        if (rulebook != null) rulebook.SetActive(false);

        dialogBox.SetActive(true);
        interrogateButton.interactable = false;

        PatientData.DialogueSet dialogueSet = currentPatient.dialogues[Random.Range(0, currentPatient.dialogues.Count)];

        doctorDialogue = dialogueSet.doctorQuestion.Replace("[name]", currentPatient.patientName)
                                                   .Replace("[symptoms]", string.Join(" and ", currentSymptoms));

        List<string> extraSymptoms = GetExtraSymptoms();
        patientDialogue = dialogueSet.patientReply.Replace("[symptoms]", string.Join(" and ", extraSymptoms));

        symptomsText.text = string.Join(", ", currentSymptoms) + ", " + string.Join(" and ", extraSymptoms);

        choiceOneButton.GetComponentInChildren<TextMeshProUGUI>().text = dialogueSet.doctorResponsePositive;
        choiceTwoButton.GetComponentInChildren<TextMeshProUGUI>().text = dialogueSet.doctorResponseNegative;

        responsePositive = dialogueSet.patientReactionPositive;
        responseNegative = dialogueSet.patientReactionNegative;

        awaitingResponse = true;
        StartCoroutine(TypeText(doctorDialogue, () => continueButton.gameObject.SetActive(true)));
    }

    private void HandleContinue()
    {
        if (awaitingResponse)
        {
            continueButton.gameObject.SetActive(false);
            StartCoroutine(TypeText(patientDialogue, () =>
            {
                choiceOneButton.gameObject.SetActive(true);
                choiceTwoButton.gameObject.SetActive(true);
            }));
            awaitingResponse = false;
        }
        else
        {
            dialogBox.SetActive(false);
        }
    }

    private void SelectResponse(bool isPositive)
    {
        choiceOneButton.gameObject.SetActive(false);
        choiceTwoButton.gameObject.SetActive(false);

        string finalResponse = isPositive ? responsePositive : responseNegative;
        StartCoroutine(TypeText(finalResponse, () =>
        {
            continueButton.gameObject.SetActive(true);
            awaitingResponse = false;
            hasAsked = true;
        }));
    }

    /* -------------------- No Test Reaction -------------------- */
    public void TriggerNoTestReaction()
    {
        string noTestDialogue;

        switch (currentPatient.personalityCategory)
        {
            case PatientData.PersonalityCategory.Positive:
                noTestDialogue = "Oh! It's good to be confident doc, but… are you sure?";
                break;
            case PatientData.PersonalityCategory.Neutral:
                noTestDialogue = "Um… shouldn’t you run some tests first?";
                break;
            case PatientData.PersonalityCategory.Negative:
                noTestDialogue = "What? You didn’t even test anything! Are you serious?";
                break;
            default:
                noTestDialogue = "Wait... you didn’t run any tests? Are you just guessing?";
                break;
        }

        dialogBox.SetActive(true);
        dialogueText.text = "";
        StartCoroutine(TypeText(noTestDialogue, () =>
        {
            continueButton.gameObject.SetActive(true);
        }));
    }

    /* -------------------- Extra Symptoms -------------------- */
    private List<string> GetExtraSymptoms()
    {
        List<string> extraSymptoms = new List<string>();

        DiseaseData disease = DiagnosisManager.Instance.GetAssignedDisease();
        if (disease != null)
        {
            List<string> allSymptoms = new List<string>(disease.symptoms);
            allSymptoms.RemoveAll(s => currentSymptoms.Contains(s));

            int patientMentionCount = Random.Range(1, Mathf.Min(3, allSymptoms.Count + 1));

            while (extraSymptoms.Count < patientMentionCount && allSymptoms.Count > 0)
            {
                string symptom = allSymptoms[Random.Range(0, allSymptoms.Count)];
                extraSymptoms.Add(symptom);
                allSymptoms.Remove(symptom);
            }
        }

        return extraSymptoms;
    }

    /* -------------------- UI Helpers -------------------- */
    private void ResetUI()
    {
        dialogBox.SetActive(false);
        continueButton.gameObject.SetActive(false);
        choiceOneButton.gameObject.SetActive(false);
        choiceTwoButton.gameObject.SetActive(false);
    }

    /* -------------------- Text Typing Effect -------------------- */
    private IEnumerator TypeText(string text, System.Action callback)
    {
        dialogueText.text = "";
        foreach (char letter in text)
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(0.02f);
        }
        callback?.Invoke();
    }
}
