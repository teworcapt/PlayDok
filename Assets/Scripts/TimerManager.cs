using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class TimerManager : MonoBehaviour
{
    public static TimerManager Instance { get; private set; }

    [Header("Day Timer")]
    [SerializeField] private float baseTime = 660f;
    [SerializeField] private float dayTimer;
    private bool isPatientProcessing;
    // Add a new field to track if final diagnosis is pending
    private bool finalDiagnosisPending = false;

    [Header("Test Timers")]
    public float testDuration = 5f;
    private readonly List<TestTimer> activeTests = new List<TestTimer>();

    [Header("UI Elements")]
    [SerializeField] private TMP_Text dayTimerTMP;
    [SerializeField] private TMP_Text dayOfWeekTMP;

    private int currentDay;
    private Color redColor;
    private Color greenColor;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        ColorUtility.TryParseHtmlString("#ff3629", out redColor);
        ColorUtility.TryParseHtmlString("#a1cd3a", out greenColor);
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
        else if (!finalDiagnosisPending && !isPatientProcessing && !AreTestsStillRunning())
        {
            // Mark that we're waiting for final diagnosis instead of ending day immediately
            finalDiagnosisPending = true;

            // Notify the player that time is up but they need to diagnose current patient
            NotificationManager.Instance?.ShowNotification("Time's up! Please diagnose current patient", redColor, NotificationType.IncorrectDiagnosis);
        }

        UpdateTestTimers();
    }

    private void EndDay()
    {
        if (SaveManager.GetDayIndex() < 6)
        {
            SceneManager.LoadScene("EndOfDay");
        }
    }

    // Add a method to call when diagnosis is complete
    public void FinalDiagnosisComplete()
    {
        if (finalDiagnosisPending)
        {
            finalDiagnosisPending = false;
            EndDay();
        }
    }

    public void StartPatientProcessing() => isPatientProcessing = true;

    public void CompletePatientProcessing()
    {
        isPatientProcessing = false;
        if (dayTimer <= 0 && !AreTestsStillRunning() && !IsPatientStillActive()) EndDay();
    }

    public void ExtendDayTimer(float additionalTime)
    {
        dayTimer += additionalTime;
        UpdateDayTimerUI();
        NotificationManager.Instance?.ShowNotification($"+{additionalTime}s", greenColor, NotificationType.TimeBoost);
    }

    public void ExtendCurrentDayTimer(float boostAmount)
    {
        if (boostAmount > 0)
        {
            dayTimer += boostAmount;
            UpdateDayTimerUI();
        }
    }

    public void ApplyPermanentTimeBoost(float boostAmount)
    {
        if (boostAmount > 0)
        {
            baseTime += boostAmount;
            dayTimer = baseTime;
            UpdateDayTimerUI();
        }
    }

    public void ApplyPenalty(float penaltyTime)
    {
        dayTimer = Mathf.Max(0, dayTimer - penaltyTime);
        UpdateDayTimerUI();
        NotificationManager.Instance?.ShowNotification($"-{penaltyTime}s", redColor, NotificationType.Penalty);
    }

    public float GetRemainingDayTime() => dayTimer;

    public void StartTestTimer(string testName)
    {
        if (activeTests.Exists(t => t.testName == testName)) return;

        DiseaseData diseaseData = PatientManager.Instance.GetCurrentPatient()?.diseaseData;
        activeTests.Add(new TestTimer(testName, testDuration, diseaseData));
        DiagnosticsManager.Instance?.UpdateTestTimerUI(testName, Mathf.CeilToInt(testDuration));
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

    public bool AreTestsStillRunning() => activeTests.Count > 0;

    private bool IsPatientStillActive()
    {
        return PatientManager.Instance != null && PatientManager.Instance.GetCurrentPatient() != null;
    }

    private void CheckPenaltyResets()
    {
        if (SaveManager.GetDayIndex() != currentDay)
        {
            PlayerStats.Instance?.ResetDailyStats();
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