using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class TimerManager : MonoBehaviour
{
    public static TimerManager Instance { get; private set; }

    [Header("Day Timer")]
    [SerializeField] private float baseTime = 660f;
    private float dayTimer;
    private bool isPatientProcessing;

    [Header("Test Timers")]
    public float testDuration = 5f;
    private readonly List<TestTimer> activeTests = new List<TestTimer>();

    [Header("UI Elements")]
    [SerializeField] private TMP_Text dayTimerTMP;
    [SerializeField] private TMP_Text dayOfWeekTMP;

    private int currentDay;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
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

    private void EndDay()
    {
        Debug.Log("Day has ended. Transitioning to EndOfDay scene...");
        if (SaveManager.GetDayIndex() < 6)
        {
            SceneManager.LoadScene("EndOfDay");
        }
    }

    public void StartPatientProcessing() => isPatientProcessing = true;

    public void CompletePatientProcessing()
    {
        isPatientProcessing = false;
        if (dayTimer <= 0) EndDay();
    }

    public void ExtendDayTimer(float additionalTime)
    {
        dayTimer += additionalTime;
        Debug.Log($"Day timer extended by {additionalTime} seconds. New total: {dayTimer}");
        UpdateDayTimerUI();
    }

    public void ApplyPermanentTimeBoost(float boostAmount)
    {
        if (boostAmount <= 0)
        {
            Debug.LogWarning("Invalid time boost value.");
            return;
        }

        baseTime += boostAmount;
        dayTimer = baseTime;
        Debug.Log($"Applied Permanent Time Boost: {boostAmount}. New base time: {baseTime}");
        UpdateDayTimerUI();
    }

    public void ApplyPenalty(float penaltyTime)
    {
        dayTimer = Mathf.Max(0, dayTimer - penaltyTime);
        Debug.Log($"Penalty applied: -{penaltyTime} seconds. Remaining time: {dayTimer}");
        UpdateDayTimerUI();
    }

    public float GetRemainingDayTime() => dayTimer;

    public void StartTestTimer(string testName)
    {
        if (activeTests.Exists(t => t.testName == testName))
        {
            Debug.Log($"Test '{testName}' is already in progress.");
            return;
        }

        DiseaseData diseaseData = PatientManager.Instance.GetCurrentPatient()?.diseaseData;
        activeTests.Add(new TestTimer(testName, testDuration, diseaseData));

        DiagnosticsManager.Instance?.UpdateTestTimerUI(testName, Mathf.CeilToInt(testDuration));
        Debug.Log($"Test '{testName}' started. Duration: {testDuration} seconds.");
    }

    private void UpdateTestTimers()
    {
        for (int i = activeTests.Count - 1; i >= 0; i--)
        {
            TestTimer test = activeTests[i];
            test.timer -= Time.deltaTime;

            DiagnosticsManager.Instance?.UpdateTestTimerUI(test.testName, Mathf.CeilToInt(test.timer));

            if (test.timer <= 0)
            {
                DiagnosticsManager.Instance?.CompleteTest(test.testName, test.diseaseData);
                activeTests.RemoveAt(i);
            }
        }
    }

    private void CheckPenaltyResets()
    {
        if (SaveManager.GetDayIndex() != currentDay)
        {
            PlayerStats.Instance?.ResetDailyStats();
            Debug.Log("Daily stats reset.");
        }
    }

    private void UpdateDayTimerUI()
    {
        if (dayTimerTMP == null) return;

        dayTimerTMP.text = dayTimer <= 0
            ? "Time Out"
            : $"{Mathf.FloorToInt(dayTimer / 60):00}:{Mathf.FloorToInt(dayTimer % 60):00}";
    }

    private void UpdateDayOfWeekUI()
    {
        if (dayOfWeekTMP == null) return;

        string currentDayName = !string.IsNullOrEmpty(LoadSaveManager.CurrentLoadedDay)
            ? LoadSaveManager.CurrentLoadedDay
            : SaveManager.GetCurrentDay();

        dayOfWeekTMP.text = currentDayName;
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
}

public class TestTimer
{
    public string testName;
    public float timer;
    public DiseaseData diseaseData;

    public TestTimer(string name, float duration, DiseaseData disease)
    {
        testName = name;
        timer = duration;
        diseaseData = disease;
    }
}
