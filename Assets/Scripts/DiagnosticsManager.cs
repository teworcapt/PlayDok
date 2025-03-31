using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class DiagnosticsManager : MonoBehaviour
{
    public static DiagnosticsManager Instance;
    public RectTransform dropZone;

    [Header("Test UI Elements")]
    private List<string> testNames = new List<string> { "Saliva Test", "Stool Test", "Urine Test", "X-ray", "Blood Test", "Temperature" };
    [SerializeField] private List<TMP_Text> testUIElements;

    private List<TestItem> activeTests = new List<TestItem>();

    /* -------------------- Singleton Setup -------------------- */
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

        InitializeTestUI();
    }

    /* -------------------- UI Initialization -------------------- */
    private void InitializeTestUI()
    {
        if (testUIElements.Count != testNames.Count)
        {
            Debug.LogError("Mismatch between testNames and testUIElements. Ensure all UI elements are assigned in Inspector.");
        }

        foreach (TMP_Text text in testUIElements)
        {
            if (text != null)
            {
                text.text = "Pending";
            }
        }
    }

    /* -------------------- Test Handling -------------------- */
    public void PerformTest(string testName, TestItem testItem)
    {
        if (testItem.IsTested) return;

        int index = testNames.IndexOf(testName);
        if (index == -1 || index >= testUIElements.Count || testUIElements[index] == null)
        {
            Debug.LogError($"[{testName}] Test UI element not assigned.");
            return;
        }

        if (activeTests.Contains(testItem))
        {
            Debug.Log($"[{testName}] Test already in progress.");
            return;
        }

        activeTests.Add(testItem);
        TimerManager.Instance.StartTestTimer(testName);
        UpdateTestUI(testName, $"Time Left: {Mathf.CeilToInt(TimerManager.Instance.testDuration)}");

        Debug.Log($"[{testName}] Test started.");
    }

    public void CompleteTest(string testName, bool isPositive)
    {
        int index = testNames.IndexOf(testName);
        if (index == -1 || testUIElements[index] == null)
        {
            Debug.LogError($"[{testName}] Test UI not assigned for '{testName}'.");
            return;
        }

        testUIElements[index].text = isPositive ? "Positive" : "Negative";
        Debug.Log($"[{testName}] Test completed. Result: {testUIElements[index].text}");

        TestItem testItem = activeTests.Find(t => t.testName == testName);
        if (testItem != null)
        {
            testItem.MarkAsTested();
            activeTests.Remove(testItem);
        }
        else
        {
            Debug.LogError($"[{testName}] TestItem not found in active tests.");
        }
    }

    /* -------------------- UI Updates -------------------- */
    public void UpdateTestTimerUI(string testName, int timeLeft)
    {
        UpdateTestUI(testName, $"Time Left: {timeLeft}");
    }

    private void UpdateTestUI(string testName, string newText)
    {
        int index = testNames.IndexOf(testName);
        if (index != -1 && testUIElements[index] != null)
        {
            testUIElements[index].text = newText;
        }
        else
        {
            Debug.LogError($"UI element not found for test: {testName}");
        }
    }

    /* -------------------- Test Zone Detection -------------------- */
    public bool IsOverDropZone(TestItem testItem)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(dropZone, testItem.transform.position, null);
    }

    /* -------------------- Diagnostics Data Access -------------------- */
    public List<TestItem> GetActiveTests()
    {
        return activeTests;
    }

    /* -------------------- Test Completion Check -------------------- */
    public bool HasCompletedTests()
    {
        return activeTests.Count == 0; 
    }

    /* -------------------- Reset System -------------------- */
    public void ResetDiagnostics()
    {
        activeTests.Clear();

        foreach (TMP_Text text in testUIElements)
        {
            if (text != null)
            {
                text.text = "Pending";
            }
        }
    }
}
