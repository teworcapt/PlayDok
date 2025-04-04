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

    private int currentDay;

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
        currentDay = SaveManager.GetDayIndex();

        dayTimer = baseTime + data.permanentTimeBoost;
        UpdateDayTimerUI();
        UpdateDayOfWeekUI();
        CheckPenaltyResets();
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
        if (SaveManager.GetDayIndex() < 6)
        {
            SceneManager.LoadScene("EndOfDay");
        }
    }

    /* -------------------- Patient Processing -------------------- */
    public void StartPatientProcessing() => isPatientProcessing = true;

    public void CompletePatientProcessing()
    {
        isPatientProcessing = false;
        if (dayTimer <= 0)
        {
            EndDay();
        }
    }

    /* -------------------- Day Timer Management -------------------- */
    public void ExtendDayTimer(float additionalTime)
    {
        dayTimer += additionalTime;
        Debug.Log($"Day timer extended by {additionalTime} seconds. New total: {dayTimer}");
        UpdateDayTimerUI();
    }

    public void ApplyPermanentTimeBoost(float boostAmount)
    {
        if (boostAmount > 0)
        {
            baseTime += boostAmount;
            dayTimer = baseTime;
            Debug.Log($"Applied Permanent Time Boost: {boostAmount}. New base time: {baseTime}");
            UpdateDayTimerUI();
        }
        else
        {
            Debug.LogWarning("Invalid time boost value.");
        }
    }

    public void ExtendCurrentDayTimer(float boostAmount)
    {
        if (boostAmount > 0)
        {
            dayTimer += boostAmount;
            Debug.Log($"Extended current day timer by {boostAmount} seconds. New total: {dayTimer}");
            UpdateDayTimerUI();
        }
        else
        {
            Debug.LogWarning("Invalid time boost value.");
        }
    }

    public void ApplyPenalty(float penaltyTime)
    {
        dayTimer = Mathf.Max(0, dayTimer - penaltyTime);
        UpdateDayTimerUI();
        Debug.Log($"Penalty applied: -{penaltyTime} seconds. Remaining time: {dayTimer}");
    }

    public float GetRemainingDayTime() => dayTimer;

    /* -------------------- Test Timer Management -------------------- */
    public void StartTestTimer(string testName)
    {
        if (activeTests.Exists(t => t.testName == testName))
        {
            Debug.Log($"Test '{testName}' is already in progress.");
            return;
        }

        activeTests.Add(new TestTimer(testName, testDuration));
        DiagnosticsManager.Instance?.UpdateTestTimerUI(testName, Mathf.CeilToInt(testDuration));

        Debug.Log($"Test '{testName}' started. Duration: {testDuration} seconds.");
    }

    private void UpdateTestTimers()
    {
        for (int i = activeTests.Count - 1; i >= 0; i--)
        {
            activeTests[i].timer -= Time.deltaTime;
            DiagnosticsManager.Instance?.UpdateTestTimerUI(activeTests[i].testName, Mathf.CeilToInt(activeTests[i].timer));

            if (activeTests[i].timer <= 0)
            {
                bool isPositive = DiagnosisManager.Instance?.IsTestPositive(activeTests[i].testName) ?? false;
                DiagnosticsManager.Instance?.CompleteTest(activeTests[i].testName, isPositive);
                activeTests.RemoveAt(i);
            }
        }
    }

    /* -------------------- Penalty Reset Management -------------------- */
    private void CheckPenaltyResets()
    {
        int savedDayIndex = SaveManager.GetDayIndex();

        if (savedDayIndex != currentDay)
        {
            PlayerStats.Instance?.ResetDailyStats();
            Debug.Log("Daily stats reset.");
        }

        if (currentDay % 7 == 0)
        {
            PlayerStats.Instance?.ResetWeeklyStats();
            Debug.Log("Weekly stats reset.");
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
