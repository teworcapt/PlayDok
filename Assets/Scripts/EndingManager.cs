using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class EndingManager : MonoBehaviour
{
    public static EndingManager Instance;

    [Header("Main Menu Scene")]
    public string mainMenuScene = "MainMenu";

    [Header("UI References")]
    public TMP_Text dialogueText;
    public TMP_Text endingText;
    public TMP_Text curedPatientsText;
    public Button continueButton;

    [Header("Dialogue Timing")]
    public float dialogueDisplayTime = 2f;

    [Header("Letter-by-Letter Settings")]
    public float letterDelay = 0.05f;

    [Header("Ending Music Clips")]
    public AudioClip goodEndingMusic;
    public AudioClip badEndingMusic;
    public AudioClip secretEndingMusic;

    [Header("Testing Options")]
    public bool overridePatientsCured = false;
    public int testPatientsCured = 0;

    [Header("Dialogue Texts")]
    [TextArea(5, 10)] public string[] goodDialogue;
    [TextArea(5, 10)] public string[] badDialogue;
    [TextArea(5, 10)] public string[] secretDialogue;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        SaveManager.LoadGame(SaveManager.GetCurrentDayIndex());
        continueButton.gameObject.SetActive(false);
        endingText.gameObject.SetActive(false);
        curedPatientsText.gameObject.SetActive(false);
        continueButton.onClick.AddListener(OnReturnToMainMenu);
        SetupEnding();
    }

    /* -------------------- Setup Ending -------------------- */
    void SetupEnding()
    {
        int cured = overridePatientsCured ? testPatientsCured :
                    (PlayerStats.Instance != null ? PlayerStats.Instance.totalCuredPatients : 0);

        if (AudioManager.Instance != null)
        {
            if (cured >= 35)
            {
                AudioManager.Instance.PlayMusic(secretEndingMusic, -1f, 1f);
                StartCoroutine(PlayDialogue(secretDialogue, "Secret Ending"));
            }
            else if (cured >= 27)
            {
                AudioManager.Instance.PlayMusic(goodEndingMusic, -1f, 1f);
                StartCoroutine(PlayDialogue(goodDialogue, "Good Ending"));
            }
            else
            {
                AudioManager.Instance.PlayMusic(badEndingMusic, -1f, 1f);
                StartCoroutine(PlayDialogue(badDialogue, "Bad Ending"));
            }
        }
        else
        {
            Debug.LogWarning("AudioManager not found — skipping music for testing.");

            if (cured >= 100)
                StartCoroutine(PlayDialogue(secretDialogue, "Secret Ending"));
            else if (cured >= 60)
                StartCoroutine(PlayDialogue(goodDialogue, "Good Ending"));
            else
                StartCoroutine(PlayDialogue(badDialogue, "Bad Ending"));
        }
    }

    /* -------------------- Play Dialogue -------------------- */
    IEnumerator PlayDialogue(string[] lines, string endingMessage)
    {
        dialogueText.text = "";
        foreach (string line in lines)
        {
            yield return StartCoroutine(TypeText(dialogueText, line, letterDelay));
            yield return new WaitForSeconds(dialogueDisplayTime);
        }

        endingText.gameObject.SetActive(true);
        yield return StartCoroutine(TypeText(endingText, endingMessage, letterDelay));

        int cured = overridePatientsCured ? testPatientsCured : PlayerStats.Instance.totalCuredPatients;
        curedPatientsText.gameObject.SetActive(true);
        yield return StartCoroutine(TypeText(curedPatientsText, $"Total cured patients: {cured}", letterDelay));

        yield return new WaitForSeconds(1.5f);
        continueButton.gameObject.SetActive(true);
        StartCoroutine(FadeInContinueText(continueButton.GetComponentInChildren<TMP_Text>(), "Click here to return to main menu...", 1.5f));
    }

    /* -------------------- Type Text (Letter-by-Letter) -------------------- */
    IEnumerator TypeText(TMP_Text target, string sentence, float delay)
    {
        target.text = "";
        foreach (char letter in sentence)
        {
            target.text += letter;
            yield return new WaitForSeconds(delay);
        }
    }

    /* -------------------- Fade In Continue Text -------------------- */
    IEnumerator FadeInContinueText(TMP_Text textComponent, string fullText, float duration)
    {
        textComponent.text = fullText;
        textComponent.alpha = 0f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            textComponent.alpha = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }
    }

    /* -------------------- Return to Main Menu -------------------- */
    public void OnReturnToMainMenu()
    {
        StartCoroutine(ReturnToMenu());
    }

    /* -------------------- Return to Main Menu Coroutine -------------------- */
    IEnumerator ReturnToMenu()
    {
        // OPTIONAL: stop ending music when returning to main menu
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMusic(1f);
        }

        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene(mainMenuScene);
    }
}
