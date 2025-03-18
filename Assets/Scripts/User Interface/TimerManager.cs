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
    public float baseDayDuration = 660f;
    public string nextSceneName;

    private float currentTestTimer = 0f;
    private bool testTimerRunning = false;

    private float currentDayTimer;
    private bool dayTimerRunning = false;
    private float extraTimePercentage = 0f;

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
        LoadDayTime();
        StartDayTimer();
    }

    private void LoadDayTime()
    {
        PlayerData data = SaveManager.LoadData();
        extraTimePercentage = data.extraTimePercentage;
        currentDayTimer = baseDayDuration + (baseDayDuration * (extraTimePercentage / 100));
    }

    public void ApplyTimeExtension(float newExtraTimePercentage)
    {
        extraTimePercentage = newExtraTimePercentage;
        currentDayTimer = baseDayDuration + (baseDayDuration * (extraTimePercentage / 100));
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

    public void ExtendDayTimer()
    {
        float extraTime = currentDayTimer * 0.005f; // 0.5% of current timer
        currentDayTimer += extraTime;

        // Save the updated time extension
        PlayerData data = SaveManager.LoadData();
        data.extraTimePercentage = extraTimePercentage;
        SaveManager.SaveData(data);

        Debug.Log("Timer extended by: " + extraTime + " seconds!");
    }

    public bool SpendCredits(int amount)
    {
        PlayerData data = SaveManager.LoadData();

        if (data.credits >= amount)
        {
            data.credits -= amount;
            SaveManager.SaveData(data);
            Debug.Log($"Spent {amount} credits. Remaining Balance: {data.credits}");
            return true;
        }
        else
        {
            Debug.Log("Yo broke.");
            return false;
        }
    }

    private void ChangeScene()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            ShopManager.Instance.AddCredits(1000); // TEMPORARY
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogWarning("No scene name set for transition.");
        }
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
}
