using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class TimerManager : MonoBehaviour
{
    public static TimerManager Instance { get; private set; }

    [Header("Day Timer")]
    [SerializeField] private float dayTimer = 660f;
    private float baseTime = 660f;
    private bool isPatientProcessing = false;

    [Header("Test Timers")]
    public float testDuration = 5f;
    private List<TestTimer> activeTests = new List<TestTimer>();

    [Header("UI Elements")]
    [SerializeField] private TMP_Text dayTimerTMP;
    [SerializeField] private TMP_Text dayOfWeekTMP;

    /* -------------------- Initialization -------------------- */
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
        PlayerData data = SaveManager.LoadData();
        dayTimer = baseTime + data.permanentTimeBoost;
        dayTimer *= (1 + data.extraTimePercentage);

        UpdateDayTimerUI();
        UpdateDayOfWeekUI();
    }

    /* -------------------- Update Loop -------------------- */
    private void Update()
    {
        if (dayTimer > 0)
        {
            dayTimer -= Time.deltaTime;
            UpdateDayTimerUI();
        }
        else if (!isPatientProcessing)
        {
            EndDay();
        }

        UpdateTestTimers();
    }

    /* -------------------- End of Day Handling -------------------- */
    private void EndDay()
    {
        Debug.Log("Day has ended. Transitioning to EndOfDay scene...");
        SceneManager.LoadScene("EndOfDay");
    }

    /* -------------------- Patient Processing -------------------- */
    public void StartPatientProcessing()
    {
        isPatientProcessing = true;
    }

    public void CompletePatientProcessing()
    {
        isPatientProcessing = false;

        if (dayTimer <= 0)
        {
            EndDay();
        }
    }

    /* -------------------- Day Timer Management -------------------- */
    public void ExtendDayTimer(float extraTime)
    {
        dayTimer += extraTime;
        UpdateDayTimerUI();
        Debug.Log($"Day timer extended by {extraTime} seconds. New total: {dayTimer}");
    }

    public void ApplyPermanentTimeBoost(float extraSeconds)
    {
        PlayerData data = SaveManager.LoadData();
        data.permanentTimeBoost += extraSeconds;
        SaveManager.SaveData(data);

        dayTimer += extraSeconds;
        UpdateDayTimerUI();
    }

    public void ApplyPenalty(float penaltyTime)
    {
        dayTimer -= penaltyTime;
        if (dayTimer < 0) dayTimer = 0;
        UpdateDayTimerUI();
        Debug.Log($"Penalty applied: -{penaltyTime} seconds. Remaining time: {dayTimer}");
    }


    public void ApplyExtraTimePercentage()
    {
        PlayerData data = SaveManager.LoadData();
        dayTimer *= (1 + data.extraTimePercentage);
        UpdateDayTimerUI();
    }

    public float GetRemainingDayTime()
    {
        return dayTimer;
    }

    /* -------------------- Test Timer Management -------------------- */
    public void StartTestTimer(string testName)
    {
        if (activeTests.Exists(t => t.testName == testName))
        {
            Debug.Log($"Test '{testName}' is already in progress.");
            return;
        }

        activeTests.Add(new TestTimer(testName, testDuration));
        DiagnosticsManager.Instance.UpdateTestTimerUI(testName, Mathf.CeilToInt(testDuration));

        Debug.Log($"Test '{testName}' started. Duration: {testDuration} seconds.");
    }

    private void UpdateTestTimers()
    {
        for (int i = activeTests.Count - 1; i >= 0; i--)
        {
            activeTests[i].timer -= Time.deltaTime;
            DiagnosticsManager.Instance.UpdateTestTimerUI(activeTests[i].testName, Mathf.CeilToInt(activeTests[i].timer));

            if (activeTests[i].timer <= 0)
            {
                bool isPositive = DiagnosisManager.Instance?.IsTestPositive(activeTests[i].testName) ?? false;
                DiagnosticsManager.Instance.CompleteTest(activeTests[i].testName, isPositive);
                activeTests.RemoveAt(i);
            }
        }
    }

    /* -------------------- UI Updates -------------------- */
    private void UpdateDayTimerUI()
    {
        if (dayTimerTMP != null)
        {
            if (dayTimer <= 0)
            {
                dayTimerTMP.text = "Time Out";
            }
            else
            {
                int minutes = Mathf.FloorToInt(dayTimer / 60);
                int seconds = Mathf.FloorToInt(dayTimer % 60);
                dayTimerTMP.text = $"{minutes:00}:{seconds:00}";
            }
        }
    }

    private void UpdateDayOfWeekUI()
    {
        if (dayOfWeekTMP != null)
        {
            dayOfWeekTMP.text = SaveManager.GetCurrentDay();
        }
    }
}

/* -------------------- Test Timer Class -------------------- */
public class TestTimer
{
    public string testName;
    public float timer;

    public TestTimer(string name, float duration)
    {
        testName = name;
        timer = duration;
    }
}
