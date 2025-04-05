using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PatientManager : MonoBehaviour
{
    /* -------------------- Singleton -------------------- */
    public static PatientManager Instance { get; private set; }

    /* -------------------- UI Elements -------------------- */
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

    [Header("Sound Effects")]
    public AudioClip dialogueLetterSFX;
    public AudioClip responseSFX;
    public int letterSFXPoolSize = 5;
    private List<AudioSource> sfxAudioSources;
    private int currentSFXIndex = 0;

    /* -------------------- Patient Data -------------------- */
    private PatientData currentPatient;
    private List<string> currentSymptoms;

    private string doctorDialogue;
    private string patientReply;
    private string doctorResponsePositive;
    private string doctorResponseNegative;
    private string patientReactionPositive;
    private string patientReactionNegative;

    private bool isWaitingForChoice;
    private bool hasPerformedTest;
    private bool isNoTestDialogActive;
    private bool isPositiveOnFirstButton;

    /* -------------------- Initialization -------------------- */
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        sfxAudioSources = new List<AudioSource>(letterSFXPoolSize);
        for (int i = 0; i < letterSFXPoolSize; i++)
        {
            var newSource = gameObject.AddComponent<AudioSource>();
            sfxAudioSources.Add(newSource);
        }

        interrogateButton.onClick.AddListener(StartInterrogation);
        continueButton.onClick.AddListener(ContinueDialog);
        choiceOneButton.onClick.AddListener(() => SelectResponse(isPositiveOnFirstButton));
        choiceTwoButton.onClick.AddListener(() => SelectResponse(!isPositiveOnFirstButton));

        ResetUI();
    }

    /* -------------------- UI Management -------------------- */
    public void UpdatePatientUI(PatientData patient, List<string> symptoms)
    {
        currentPatient = patient;
        currentSymptoms = new List<string>(symptoms);

        patientImage.sprite = patient.patientSprites[Random.Range(0, patient.patientSprites.Count)];
        patientNameHolder.text = patient.patientName;
        dialogNameHolder.text = patient.patientName;
        symptomsText.text = string.Join(", ", currentSymptoms);

        ResetUI();
        hasPerformedTest = false;
        interrogateButton.interactable = true;
    }

    private void ResetUI()
    {
        dialogBox.SetActive(false);
        continueButton.gameObject.SetActive(false);
        choiceOneButton.gameObject.SetActive(false);
        choiceTwoButton.gameObject.SetActive(false);
        isWaitingForChoice = false;
        isNoTestDialogActive = false;
        hasPerformedTest = false;
    }

    public void EnableInterrogateButton()
    {
        interrogateButton.interactable = true;
    }

    /* -------------------- Interrogation Management -------------------- */
    private void StartInterrogation()
    {
        if (dialogBox.activeSelf) return;

        rulebook?.SetActive(false);
        dialogBox.SetActive(true);
        dialogueText.text = "";
        interrogateButton.interactable = false;
        isNoTestDialogActive = false;

        RulebookManager.Instance?.SetInterrogationState(true);

        var dialogueSet = currentPatient.dialogues[Random.Range(0, currentPatient.dialogues.Count)];

        dialogNameHolder.text = "Doctor";

        doctorDialogue = dialogueSet.doctorQuestion.Replace("[name]", currentPatient.patientName)
                                                   .Replace("[symptoms]", string.Join(" and ", currentSymptoms));

        var extraSymptoms = GetExtraSymptoms();
        patientReply = dialogueSet.patientReply.Replace("[symptoms]", string.Join(" and ", extraSymptoms));

        doctorResponsePositive = dialogueSet.doctorResponsePositive;
        doctorResponseNegative = dialogueSet.doctorResponseNegative;
        patientReactionPositive = dialogueSet.patientReactionPositive.Replace("[symptoms]", string.Join(" and ", extraSymptoms));
        patientReactionNegative = dialogueSet.patientReactionNegative.Replace("[symptoms]", string.Join(" and ", extraSymptoms));

        symptomsText.text = string.Join(", ", currentSymptoms);

        StartCoroutine(TypeText(doctorDialogue, () => continueButton.gameObject.SetActive(true)));
    }

    private void ContinueDialog()
    {
        if (isNoTestDialogActive)
            ContinueNoTestDialog();
        else
            ContinueInterrogation();
    }

    private void ContinueInterrogation()
    {
        if (!isWaitingForChoice)
        {
            dialogNameHolder.text = currentPatient.patientName;

            StartCoroutine(TypeText(patientReply, () =>
            {
                isPositiveOnFirstButton = Random.Range(0, 2) == 0;

                var btn1Text = choiceOneButton.GetComponentInChildren<TextMeshProUGUI>();
                var btn2Text = choiceTwoButton.GetComponentInChildren<TextMeshProUGUI>();

                if (isPositiveOnFirstButton)
                {
                    btn1Text.text = doctorResponsePositive;
                    btn2Text.text = doctorResponseNegative;
                }
                else
                {
                    btn1Text.text = doctorResponseNegative;
                    btn2Text.text = doctorResponsePositive;
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
            RulebookManager.Instance?.SetInterrogationState(false);
            interrogateButton.interactable = false;
        }
    }

    private void ContinueNoTestDialog()
    {
        dialogBox.SetActive(false);
        RulebookManager.Instance?.SetInterrogationState(false);
        ResetUI();
    }

    private void SelectResponse(bool isPositive)
    {
        choiceOneButton.gameObject.SetActive(false);
        choiceTwoButton.gameObject.SetActive(false);

        string finalResponse = isPositive ? patientReactionPositive : patientReactionNegative;
        float timeAdjustment = currentPatient.GetTimePenalty(isPositive);

        if (isPositive)
            TimerManager.Instance?.ExtendDayTimer(timeAdjustment);
        else
            TimerManager.Instance?.ApplyPenalty(timeAdjustment);

        StartCoroutine(TypeText(finalResponse, () => continueButton.gameObject.SetActive(true)));
    }

    /* -------------------- Patient Management -------------------- */
    public void MarkTestPerformed() => hasPerformedTest = true;

    public bool CanProceedToNextPatient()
    {
        if (!hasPerformedTest)
        {
            StartCoroutine(ShowNoTestSubmitDialogCoroutine());
            return false;
        }
        return true;
    }

    private List<string> GetExtraSymptoms()
    {
        var extraSymptoms = new List<string>();
        var disease = DiagnosisManager.Instance?.GetAssignedDisease();

        if (disease == null) return extraSymptoms;

        var allSymptoms = new List<string>(disease.symptoms);
        allSymptoms.RemoveAll(s => currentSymptoms.Contains(s));

        int patientMentionCount = Random.Range(1, Mathf.Min(3, allSymptoms.Count + 1));

        while (extraSymptoms.Count < patientMentionCount && allSymptoms.Count > 0)
        {
            int index = Random.Range(0, allSymptoms.Count);
            string symptom = allSymptoms[index];
            extraSymptoms.Add(symptom);
            currentSymptoms.Add(symptom);
            allSymptoms.RemoveAt(index);
        }

        return extraSymptoms;
    }

    private IEnumerator ShowNoTestSubmitDialogCoroutine()
    {
        isNoTestDialogActive = true;

        rulebook?.SetActive(false);
        dialogBox.SetActive(true);
        interrogateButton.interactable = false;

        RulebookManager.Instance?.SetInterrogationState(true);
        dialogNameHolder.text = currentPatient.patientName;

        string noTestDialog = currentPatient.noTestSubmitLines.Count > 0
            ? currentPatient.noTestSubmitLines[Random.Range(0, currentPatient.noTestSubmitLines.Count)]
            : "You haven't run any tests yet!";

        yield return StartCoroutine(TypeText(noTestDialog, () => continueButton.gameObject.SetActive(true)));
    }

    /* -------------------- Text Typing Effect -------------------- */
    private IEnumerator TypeText(string text, System.Action callback)
    {
        dialogueText.text = "";
        foreach (char letter in text)
        {
            dialogueText.text += letter;

            if (!char.IsWhiteSpace(letter) && dialogueLetterSFX != null)
            {
                var source = sfxAudioSources[currentSFXIndex];
                source.pitch = Random.Range(0.95f, 1.05f);
                source.PlayOneShot(dialogueLetterSFX);

                currentSFXIndex = (currentSFXIndex + 1) % sfxAudioSources.Count;
            }

            yield return new WaitForSeconds(0.02f);
        }

        callback?.Invoke();
    }

    /* -------------------- Data Retrieval -------------------- */
    public PatientData GetCurrentPatient() => currentPatient;

    public bool IsTestPositive(string testName)
    {
        var data = DiagnosisManager.Instance?.GetAssignedDisease();

        if (data == null)
        {
            Debug.LogWarning("No fallback disease data available.");
            return false;
        }

        bool result = data.tests.Contains(testName);
        Debug.Log($"IsTestPositive for {testName}: {result} (disease: {data.diseaseName})");
        return result;
    }
}
