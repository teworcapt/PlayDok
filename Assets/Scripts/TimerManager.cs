using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class TimerManager : MonoBehaviour
{
    public static TimerManager Instance { get; private set; }
    [SerializeField] private float baseTime = 60f; // 1 minute
    [SerializeField] private float dayTimer;
    private bool isPatientProcessing;
    private bool finalDiagnosisPending = false;
    public float testDuration = 5f;
    private readonly List<TestTimer> activeTests = new List<TestTimer>();
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

    private void Reset()
    {
        baseTime = 60f; // Ensure it's reset to 1 minute
    }

    private void Start()
    {
        baseTime = 60f;

        int currentDayIndex = SaveManager.GetCurrentDayIndex();
        GameSaveData data = SaveManager.LoadGame(currentDayIndex);
        PlayerPersistentData persistentData = PersistentDataManager.LoadData();
        currentDay = currentDayIndex;

        float permanentBoost = PlayerStats.Instance.timeBoostPermanent;

        dayTimer = baseTime + permanentBoost;

        Debug.Log($"TimerManager: Setting day timer to {baseTime}s (base) + {permanentBoost}s (boost) = {dayTimer}s total");

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
            finalDiagnosisPending = true;
            NotificationManager.Instance?.ShowNotification("Time's up! Please diagnose current patient", redColor, NotificationType.IncorrectDiagnosis);
        }
        UpdateTestTimers();
    }

    private void EndDay()
    {
            SceneManager.LoadScene("EndOfDay");
    }

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
        if (dayTimer <= 0 && !AreTestsStillRunning() && !IsPatientStillActive())
            EndDay();
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
            Debug.Log($"Extended day timer by {boostAmount}s, new total: {dayTimer}s");
            UpdateDayTimerUI();
        }
    }

    public void ApplyPermanentTimeBoost(float boostAmount)
    {
        if (boostAmount > 0)
        {
            dayTimer += boostAmount;

            PlayerStats.Instance.AddPermanentBoost(boostAmount);

            PlayerPersistentData data = PersistentDataManager.LoadData();
            data.permanentTimeBoost += boostAmount;
            PersistentDataManager.SaveData(data);

            Debug.Log($"Applied permanent time boost of {boostAmount}s, new total: {dayTimer}s");
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
        if (activeTests.Exists(t => t.testName == testName))
            return;
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

    public void ResetPermanentTimeBoosts()
    {
        PlayerStats.Instance?.ResetPermanentTimeBoosts();
        UpdateDayTimerUI();
    }

    public bool AreTestsStillRunning() => activeTests.Count > 0;

    private bool IsPatientStillActive()
    {
        return PatientManager.Instance != null && PatientManager.Instance.GetCurrentPatient() != null;
    }

    private void CheckPenaltyResets()
    {
        if (SaveManager.GetCurrentDayIndex() != currentDay)
            PlayerStats.Instance?.ResetDailyStats();
    }

    private void UpdateDayTimerUI()
    {
        if (dayTimerTMP == null)
            return;
        dayTimerTMP.text = dayTimer <= 0 ? "Time Out" : $"{Mathf.FloorToInt(dayTimer / 60):00}:{Mathf.FloorToInt(dayTimer % 60):00}";
    }

    private void UpdateDayOfWeekUI()
    {
        if (dayOfWeekTMP == null)
            return;
        string currentDayName = LoadSaveManager.CurrentLoadedDay;
        if (string.IsNullOrEmpty(currentDayName))
            currentDayName = SaveManager.GetCurrentDay();
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