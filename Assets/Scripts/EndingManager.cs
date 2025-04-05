using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class EndingManager : MonoBehaviour
{

    public static EndingManager Instance { get; private set; }

    [Header("Penalty Settings")]
    public int penaltyCount;

    [Header("Main Menu Scene")]
    public string mainMenuScene = "MainMenu";

    [Header("UI References")]
    public Image backgroundImage;
    public TextMeshProUGUI dialogueText;
    public Button continueButton;

    [Header("Background Art")]
    public Sprite goodEndingArt;
    public Sprite badEndingArt;

    [Header("Audio")]
    public AudioSource musicSource;
    public AudioClip goodEndingMusic;
    public AudioClip badEndingMusic;

    [Header("Dialogue Timing")]
    public float dialogueDisplayTime = 3f;

    [Header("Dialogue")]
    [TextArea]
    public string[] goodDialogue = new string[]
    {
        "The doctor, the savior, as the plush babies gather in a radiant display of unity.",
        "May their love and healing hands fills the world with warmth and hope."
    };

    [TextArea]
    public string[] badDialogue = new string[]
    {
        "In a desolate world, the doctor stands amid her ruined and bereft allies.",
        "Known as the 'Destructor', their legacy is one of devastation and regret."
    };

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        continueButton.interactable = false;
        continueButton.onClick.AddListener(OnReturnToMainMenu);
        SetupEnding();
    }

    void SetupEnding()
    {
        if (penaltyCount > 5000)
        {
            backgroundImage.sprite = badEndingArt;
            musicSource.clip = badEndingMusic;
            musicSource.Play();
            StartCoroutine(PlayDialogue(badDialogue));
        }
        else
        {
            backgroundImage.sprite = goodEndingArt;
            musicSource.clip = goodEndingMusic;
            musicSource.Play();
            StartCoroutine(PlayDialogue(goodDialogue));
        }
    }

    IEnumerator PlayDialogue(string[] lines)
    {
        foreach (string line in lines)
        {
            yield return StartCoroutine(FadeInText(line));
            yield return new WaitForSeconds(dialogueDisplayTime);
        }

        continueButton.interactable = true;
        dialogueText.text = "Tap to return to main menu...";
    }

    IEnumerator FadeInText(string line)
    {
        dialogueText.text = "";
        dialogueText.alpha = 0;
        dialogueText.text = line;

        float duration = 1f;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            dialogueText.alpha = Mathf.Clamp01(timer / duration);
            yield return null;
        }

        dialogueText.alpha = 1f;
    }

    public void OnReturnToMainMenu()
    {
        StartCoroutine(ReturnToMenu());
    }

    IEnumerator ReturnToMenu()
    {
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene(mainMenuScene);
    }
}
