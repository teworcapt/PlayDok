using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class TimerManager : MonoBehaviour
{
    public static TimerManager Instance { get; private set; }

    [Header("Test Timer")]
    public TMP_Text testTimerText;
    public float testDuration = 5f;

    [Header("Day Timer")]
    public TMP_Text dayTimerText;
    public float dayDuration = 660f;
    public string nextSceneName;

    private float currentTestTimer = 0f;
    private bool testTimerRunning = false;

    private float currentDayTimer = 0f;
    private bool dayTimerRunning = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        StartDayTimer();
    }

    public void StartTestTimer()
    {
        if (!testTimerRunning)
        {
            StartCoroutine(TestTimerCountdown());
        }
    }

    private IEnumerator TestTimerCountdown()
    {
        testTimerRunning = true;
        currentTestTimer = testDuration;

        while (currentTestTimer > 0)
        {
            UpdateTestTimerUI();
            yield return new WaitForSeconds(1f);
            currentTestTimer -= 1f;
        }

        testTimerRunning = false;
        UpdateTestTimerUI();
    }

    private void UpdateTestTimerUI()
    {
        if (testTimerText != null)
        {
            int minutes = Mathf.FloorToInt(currentTestTimer / 60);
            int seconds = Mathf.FloorToInt(currentTestTimer % 60);
            testTimerText.text = $"{minutes:D2}:{seconds:D2}";
        }
    }

    public void StartDayTimer()
    {
        if (!dayTimerRunning)
        {
            StartCoroutine(DayTimerCountdown());
        }
    }

    private IEnumerator DayTimerCountdown()
    {
        dayTimerRunning = true;
        currentDayTimer = dayDuration;

        while (currentDayTimer > 0)
        {
            UpdateDayTimerUI();
            yield return new WaitForSeconds(1f);
            currentDayTimer -= 1f;
        }

        dayTimerRunning = false;
        UpdateDayTimerUI();
        ChangeScene();
    }

    private void UpdateDayTimerUI()
    {
        if (dayTimerText != null)
        {
            int minutes = Mathf.FloorToInt(currentDayTimer / 60);
            int seconds = Mathf.FloorToInt(currentDayTimer % 60);
            dayTimerText.text = $"{minutes:D2}:{seconds:D2}";
        }
    }

    private void ChangeScene()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            Debug.Log($"[TimerManager] Changing scene to {nextSceneName}");
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogWarning("[TimerManager] No scene name set for transition.");
        }
    }
}
