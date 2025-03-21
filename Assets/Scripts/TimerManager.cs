using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class TimerManager : MonoBehaviour
{
    public static TimerManager Instance { get; private set; }

    [Header("Day Timer")]
    [SerializeField] private float dayTimer = 660f;
    private float baseTime = 660f;

    [Header("Test Timer")]
    public float testDuration = 5f;
    private float testTimer;

    [Header("UI Elements")]
    [SerializeField] private TMP_Text dayTimerTMP;
    [SerializeField] private TMP_Text testTimerTMP;
    [SerializeField] private TMP_Text dayOfWeekTMP;

    private bool isPatientProcessing = false;

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
        UpdateTestTimerUI();
        UpdateDayOfWeekUI();
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

        if (testTimer > 0)
        {
            testTimer -= Time.deltaTime;
            UpdateTestTimerUI();
        }
    }

    private void EndDay()
    {
        Debug.Log("Day has ended. Transitioning to EndOfDay scene...");
        SceneManager.LoadScene("EndOfDay");
    }

    public void StartPatientProcessing()
    {
        isPatientProcessing = true;
        Debug.Log("New patient processing started.");
    }

    public void CompletePatientProcessing()
    {
        isPatientProcessing = false;
        Debug.Log("Patient processing complete.");

        if (dayTimer <= 0)
        {
            EndDay();
        }
    }

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

    public void StartTestTimer()
    {
        testTimer = testDuration;
        UpdateTestTimerUI();
        Debug.Log($"Test started. Duration: {testDuration} seconds.");
    }

    public void SetTestDuration(float duration)
    {
        testDuration = duration;
        Debug.Log($"Test duration set to {testDuration} seconds.");
    }

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

    private void UpdateTestTimerUI()
    {
        if (testTimerTMP != null)
        {
            testTimerTMP.text = $"Test Timer: {Mathf.CeilToInt(testTimer)}";
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
