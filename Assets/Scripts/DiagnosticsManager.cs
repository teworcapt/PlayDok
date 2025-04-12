using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.Collections;

public class DiagnosticsManager : MonoBehaviour
{
    public static DiagnosticsManager Instance;
    public RectTransform dropZone;

    [Header("Test UI Elements")]
    private List<string> testNames = new List<string> { "Saliva Test", "Stool Test", "Urine Test", "X-ray", "Blood Test", "Temperature" };
    [SerializeField] private List<TMP_Text> testUIElements;

    private List<TestItem> activeTests = new List<TestItem>();
    private int completedTests = 0;
    private HashSet<string> completedTestTypes = new HashSet<string>();

    public bool CanMoveToNextPatient => activeTests.Count == 0 && completedTestTypes.Count > 0;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        InitializeTestUI();
    }

    private void InitializeTestUI()
    {
        if (testUIElements.Count != testNames.Count)
            Debug.LogError("Mismatch between testNames and testUIElements. Ensure all UI elements are assigned in Inspector.");

        foreach (TMP_Text text in testUIElements)
        {
            if (text != null)
                text.text = "Pending";
        }
    }

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

        bool isPositive = PatientManager.Instance.IsTestPositive(testName);
        Debug.Log($"[{testName}] Test started. Result will be: {(isPositive ? "Positive" : "Negative")}");

        StartCoroutine(HandleTestCompletion(testItem, isPositive));
    }

    private IEnumerator HandleTestCompletion(TestItem testItem, bool isPositive)
    {
        yield return new WaitForSeconds(TimerManager.Instance.testDuration);
        testItem.MarkAsTested();
        Debug.Log($"[{testItem.testName}] Test completed. Final Result: {(isPositive ? "Positive" : "Negative")}");
        UpdateTestUI(testItem.testName, isPositive ? "Positive" : "Negative");
        activeTests.Remove(testItem);
        OnTestCompleted(testItem);
    }

    private void OnTestCompleted(TestItem testItem)
    {
        completedTestTypes.Add(testItem.testName);
        completedTests = completedTestTypes.Count;
        Debug.Log($"Completed test: {testItem.testName}. Total unique tests: {completedTests}/{testNames.Count}");
    }

    public List<TestItem> GetActiveTests()
    {
        return activeTests;
    }

    public bool HasCompletedTests()
    {
        return activeTests.Count == 0;
    }

    public void UpdateTestTimerUI(string testName, int timeLeft)
    {
        UpdateTestUI(testName, $"Time Left: {timeLeft}");
    }

    private void UpdateTestUI(string testName, string newText)
    {
        int index = testNames.IndexOf(testName);
        if (index != -1 && testUIElements[index] != null)
            testUIElements[index].text = newText;
        else
            Debug.LogError($"UI element not found for test: {testName}");
    }

    public bool IsOverDropZone(TestItem testItem)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(dropZone, testItem.transform.position, null);
    }

    public void ResetDiagnostics()
    {
        Debug.Log("[DiagnosticsManager] Resetting diagnostics for new patient.");
        activeTests.Clear();
        completedTests = 0;
        completedTestTypes.Clear();

        foreach (TMP_Text text in testUIElements)
        {
            if (text != null)
                text.text = "Pending";
        }

        foreach (TestItem testItem in Object.FindObjectsByType<TestItem>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            testItem.ResetState();
        }
    }

    public void CompleteTest(string testName, DiseaseData diseaseData)
    {
        int index = testNames.IndexOf(testName);
        if (index == -1 || testUIElements[index] == null)
        {
            Debug.LogError($"[{testName}] Test UI not assigned for '{testName}'.");
            return;
        }

        bool isPositive = diseaseData != null && diseaseData.tests.Contains(testName);
        testUIElements[index].text = isPositive ? "Positive" : "Negative";
        Debug.Log($"[{testName}] Test completed. Result: {testUIElements[index].text}");

        TestItem testItem = activeTests.Find(item => item.testName == testName);
        if (testItem != null)
        {
            testItem.MarkAsTested();
            testItem.SetTestResult(isPositive);
            activeTests.Remove(testItem);
            OnTestCompleted(testItem);
        }
        else
        {
            TestItem dummyItem = new TestItem();
            dummyItem.testName = testName;
            OnTestCompleted(dummyItem);
        }
    }
}