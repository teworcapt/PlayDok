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

    private string doctorDialogue;
    private string patientReply;
    private string doctorResponsePositive;
    private string doctorResponseNegative;
    private string patientReactionPositive;
    private string patientReactionNegative;

    private bool isWaitingForChoice = false;

    /* -------------------- Initialization -------------------- */
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        interrogateButton.onClick.AddListener(StartInterrogation);
        continueButton.onClick.AddListener(ContinueDialog);
        choiceOneButton.onClick.AddListener(() => SelectResponse(true));
        choiceTwoButton.onClick.AddListener(() => SelectResponse(false));

        ResetUI();
    }

    /* -------------------- Patient Setup -------------------- */
    public void UpdatePatientUI(PatientData patient, List<string> symptoms)
    {
        currentPatient = patient;
        currentSymptoms = new List<string>(symptoms);

        patientImage.sprite = patient.patientSprites[Random.Range(0, patient.patientSprites.Count)];
        patientNameHolder.text = patient.patientName;
        dialogNameHolder.text = patient.patientName;
        symptomsText.text = string.Join(", ", currentSymptoms);

        ResetUI();
    }

    /* -------------------- Interrogation Flow -------------------- */
    private void StartInterrogation()
    {
        if (rulebook != null) rulebook.SetActive(false);

        dialogBox.SetActive(true);
        interrogateButton.interactable = false;

        RulebookManager.Instance.SetInterrogationState(true);

        PatientData.DialogueSet dialogueSet = currentPatient.dialogues[Random.Range(0, currentPatient.dialogues.Count)];

        dialogNameHolder.text = "Doctor";

        doctorDialogue = dialogueSet.doctorQuestion.Replace("[name]", currentPatient.patientName)
                                                   .Replace("[symptoms]", string.Join(" and ", currentSymptoms));

        List<string> extraSymptoms = GetExtraSymptoms();
        patientReply = dialogueSet.patientReply.Replace("[symptoms]", string.Join(" and ", extraSymptoms));

        doctorResponsePositive = dialogueSet.doctorResponsePositive;
        doctorResponseNegative = dialogueSet.doctorResponseNegative;

        patientReactionPositive = dialogueSet.patientReactionPositive.Replace("[symptoms]", string.Join(" and ", extraSymptoms));
        patientReactionNegative = dialogueSet.patientReactionNegative.Replace("[symptoms]", string.Join(" and ", extraSymptoms));

        StartCoroutine(TypeText(doctorDialogue, () => continueButton.gameObject.SetActive(true)));
    }

    private void ContinueDialog()
    {
        if (!isWaitingForChoice)
        {
            dialogNameHolder.text = currentPatient.patientName;
            StartCoroutine(TypeText(patientReply, () =>
            {
                bool positiveOnFirstButton = Random.Range(0, 2) == 0;

                if (positiveOnFirstButton)
                {
                    choiceOneButton.GetComponentInChildren<TextMeshProUGUI>().text = doctorResponsePositive;
                    choiceTwoButton.GetComponentInChildren<TextMeshProUGUI>().text = doctorResponseNegative;
                }
                else
                {
                    choiceOneButton.GetComponentInChildren<TextMeshProUGUI>().text = doctorResponseNegative;
                    choiceTwoButton.GetComponentInChildren<TextMeshProUGUI>().text = doctorResponsePositive;
                }

                choiceOneButton.gameObject.SetActive(true);
                choiceTwoButton.gameObject.SetActive(true);
                isWaitingForChoice = true;
            }));

            continueButton.gameObject.SetActive(false);
        }
        else
        {
            dialogBox.SetActive(false);

            RulebookManager.Instance.SetInterrogationState(false);

            interrogateButton.interactable = false;
        }
    }


    private void SelectResponse(bool isPositive)
    {
        choiceOneButton.gameObject.SetActive(false);
        choiceTwoButton.gameObject.SetActive(false);

        string finalResponse = isPositive ? patientReactionPositive : patientReactionNegative;
        float timeAdjustment = currentPatient.GetTimePenalty(isPositive);

        if (isPositive)
        {
            TimerManager.Instance.ExtendDayTimer(timeAdjustment);
        }
        else
        {
            TimerManager.Instance.ApplyPenalty(timeAdjustment);
        }

        StartCoroutine(TypeText(finalResponse, () =>
        {
            continueButton.gameObject.SetActive(true);
        }));
    }

    public void EnableInterrogateButton()
    {
        interrogateButton.interactable = true;
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
        isWaitingForChoice = false;
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
