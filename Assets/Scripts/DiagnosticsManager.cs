using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class DiagnosticsManager : MonoBehaviour
{
    public static DiagnosticsManager Instance;
    public RectTransform dropZone;

    private List<TestItem> activeTests = new List<TestItem>();

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

    public void PerformTest(string testName, TestItem testItem)
    {
        if (activeTests.Contains(testItem))
        {
            Debug.Log($"[{testName}] Test already in progress.");
            return;
        }

        bool isPositive = PatientManager.Instance.IsTestPositive(testName);
        Debug.Log($"[{testName}] Test started. Result: {(isPositive ? "Positive" : "Negative")}");

        activeTests.Add(testItem);

        // Start test timer
        TimerManager.Instance.StartTestTimer();

        StartCoroutine(HandleTestCompletion(testItem, isPositive));
    }

    private IEnumerator HandleTestCompletion(TestItem testItem, bool isPositive)
    {
        yield return new WaitForSeconds(TimerManager.Instance.testDuration);
        testItem.MarkAsTested();
        Debug.Log($"[{testItem.testName}] Test completed. Final Result: {(isPositive ? "Positive" : "Negative")}");
        activeTests.Remove(testItem);
    }

    public bool IsOverDropZone(TestItem testItem)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(dropZone, testItem.transform.position, null);
    }

    public void ResetDiagnostics()
    {
        Debug.Log("[DiagnosticsManager] Resetting diagnostics for new patient.");

        foreach (TestItem test in activeTests)
        {
            test.ResetTest();
        }
        activeTests.Clear();
    }
}
