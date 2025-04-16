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

    [Header("Audio Settings")]
    [Range(0.1f, 1.0f)]
    public float dialogLetterVolume = 0.5f;
    [Range(0.01f, 0.1f)]
    public float typeEffectSpeed = 0.02f;
    [Tooltip("Random pitch variation for dialog sounds")]
    public bool randomizeDialogPitch = true;

    [Header("Patient Pool")]
    public List<PatientData> allPatients = new List<PatientData>();
    private List<PatientData> availablePatients = new List<PatientData>();

    /* -------------------- Patient Data -------------------- */
    private PatientData currentPatient;
    private List<string> currentSymptoms;

    // Dialogue and state variables.
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

    public BlinkController blinkController;

    /* -------------------- Initialization -------------------- */
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        ResetUI();
        SetupButtonListeners();
        InitializePatientPool();
    }

    private void InitializePatientPool()
    {
        // Set up availablePatients with all available patients (refill as needed).
        availablePatients = new List<PatientData>(allPatients);
    }

    private void SetupButtonListeners()
    {
        if (interrogateButton != null)
        {
            interrogateButton.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlayButtonClickSound();
                StartInterrogation();
            });
        }

        if (continueButton != null)
        {
            continueButton.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlayButtonClickSound();
                ContinueDialog();
            });
        }

        if (choiceOneButton != null)
        {
            choiceOneButton.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlayButtonClickSound();
                SelectResponse(isPositiveOnFirstButton);
            });
        }

        if (choiceTwoButton != null)
        {
            choiceTwoButton.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlayButtonClickSound();
                SelectResponse(!isPositiveOnFirstButton);
            });
        }
    }

    /* -------------------- UI Management -------------------- */
    public void UpdatePatientUI(PatientData patient, List<string> symptoms)
    {
        currentPatient = patient;
        currentSymptoms = new List<string>(symptoms);

        patientImage.sprite = patient.selectedSpriteSet.normalSprite;

        patientNameHolder.text = patient.patientName;
        dialogNameHolder.text = patient.patientName;
        symptomsText.text = string.Join(", ", currentSymptoms);

        var blinkController = patientImage.GetComponent<BlinkController>();
        if (blinkController != null)
        {
            blinkController.SetPatient(patient);
        }

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

        var dialogueSet = currentPatient.GetNextDialogueSet();

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

    /* -------------------- Extra Symptoms Method -------------------- */
    public void MarkTestPerformed()
    {
        hasPerformedTest = true;

        AudioManager.Instance.PlayPopSound();
    }

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
    public IEnumerator ShowNoTestSubmitDialogCoroutine()
    {
        isNoTestDialogActive = true;

        rulebook?.SetActive(false);
        dialogBox.SetActive(true);

        RulebookManager.Instance?.SetInterrogationState(true);

        dialogNameHolder.text = currentPatient.patientName;

        string noTestDialog = currentPatient.noTestSubmitLines.Count > 0
            ? currentPatient.noTestSubmitLines[Random.Range(0, currentPatient.noTestSubmitLines.Count)]
            : "You haven't run any tests yet!";

        yield return StartCoroutine(TypeText(noTestDialog, () =>
        {
            continueButton.gameObject.SetActive(true);
        }));
    }


    /* -------------------- Text Typing Effect -------------------- */
    private IEnumerator TypeText(string text, System.Action callback)
    {
        dialogueText.text = "";
        foreach (char letter in text)
        {
            dialogueText.text += letter;
            if (!char.IsWhiteSpace(letter))
            {
                AudioManager.Instance.PlayDialogLetterSound(dialogLetterVolume);
            }
            yield return new WaitForSeconds(typeEffectSpeed);
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
