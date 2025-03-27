using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class DiagnosticsManager : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
<<<<<<< Updated upstream
    public string testName;
    public GameObject positivePrefab;
    public GameObject negativePrefab;
    public GameObject timerTextPrefab;
    public Transform patientDropzone;
    public float timerDuration = 5f;

    private RectTransform rectTransform;
    private Vector2 originalPosition;
    private bool isTimerRunning = false;
    private bool isCompleted = false;
    private GameObject currentTimerText;
    private PatientData currentPatient;
    private PatientDropzone dropzoneScript;

    private GameObject currentResultPrefab;
=======
    public static DiagnosticsManager Instance { get; private set; }

    public RectTransform dropZone;
    private List<TestItem> activeTests = new List<TestItem>();
    private Dictionary<string, TMP_Text> testResultTexts = new Dictionary<string, TMP_Text>();
>>>>>>> Stashed changes

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalPosition = rectTransform.anchoredPosition;

        if (patientDropzone != null)
        {
            dropzoneScript = patientDropzone.GetComponent<PatientDropzone>();
        }
        else
        {
            Debug.LogError("❌ PatientDropzone is NOT assigned in the Inspector!");
        }

        FindTestResultTexts();
    }

    private void FindTestResultTexts()
    {
        testResultTexts.Clear();
        TMP_Text[] allTexts = Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        string[] testNames = { "Saliva", "Urine", "Stool", "Xray", "Syringe", "Thermometer" };

        foreach (string testName in testNames)
        {
            foreach (TMP_Text tmp in allTexts)
            {
                if (tmp.name == testName)
                {
                    testResultTexts[testName] = tmp;
                    break;
                }
            }

            if (!testResultTexts.ContainsKey(testName))
            {
                Debug.LogWarning($"[DiagnosticsManager] Test result TMP '{testName}' not found!");
            }
        }
    }

    public bool IsTestPositive(string testName)
    {
        if (DiseaseManager.Instance == null)
        {
            Debug.LogError("[DiagnosticsManager] DiseaseManager instance is missing!");
            return false;
        }

        string currentDiseaseName = DiagnosisManager.Instance?.GetCurrentDisease();
        if (string.IsNullOrEmpty(currentDiseaseName))
        {
            Debug.LogError("[DiagnosticsManager] No current disease selected!");
            return false;
        }

        DiseaseData currentDisease = DiseaseManager.Instance.GetDiseaseData(currentDiseaseName);
        if (currentDisease == null)
        {
            Debug.LogError($"[DiagnosticsManager] Disease '{currentDiseaseName}' not found in DiseaseManager!");
            return false;
        }

        return currentDisease.tests.Contains(testName);
    }

<<<<<<< Updated upstream
    public void ResetDiagnostics()
    {
        isTimerRunning = false;
        isCompleted = false;
        currentPatient = null;

        if (currentResultPrefab != null)
        {
            Destroy(currentResultPrefab);
            currentResultPrefab = null;
            Debug.Log("Result prefab destroyed during reset.");
        }

        if (currentTimerText != null)
        {
            Destroy(currentTimerText);
            currentTimerText = null;
        }

        rectTransform.anchoredPosition = originalPosition;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isTimerRunning || isCompleted) return;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isTimerRunning || isCompleted) return;
        Vector2 movePosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent as RectTransform,
            eventData.position, eventData.pressEventCamera, out movePosition);
        rectTransform.anchoredPosition = movePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isTimerRunning || isCompleted) return;

        if (RectTransformUtility.RectangleContainsScreenPoint(patientDropzone.GetComponent<RectTransform>(), eventData.position))
        {
            Debug.Log("Dropped on patient! Starting timer...");
            rectTransform.anchoredPosition = originalPosition;

            if (dropzoneScript != null)
            {
                currentPatient = dropzoneScript.GetCurrentPatient();
            }

            if (currentPatient != null)
            {
                StartCoroutine(StartTimer());
            }
            else
            {
                Debug.LogError("Current patient is NULL! Timer will not start.");
            }
        }
        else
        {
            Debug.Log("Dropped outside patient.");
            rectTransform.anchoredPosition = originalPosition;
        }
    }

    private IEnumerator StartTimer()
    {
        isTimerRunning = true;

        if (timerTextPrefab == null)
        {
            Debug.LogError("TimerTextPrefab is NOT assigned in the Inspector!");
            yield break;
        }

        if (currentTimerText == null)
        {
            Debug.Log("Instantiating Timer Prefab...");
            currentTimerText = Instantiate(timerTextPrefab, transform.parent);
            RectTransform timerRect = currentTimerText.GetComponent<RectTransform>();
            if (timerRect != null)
            {
                timerRect.anchoredPosition = new Vector2(0, -100);
            }
            else
            {
                Debug.LogError("Timer Prefab is missing a RectTransform!");
            }
        }

        float timeRemaining = timerDuration;
        TextMeshProUGUI timerTextComponent = currentTimerText.GetComponent<TextMeshProUGUI>();

        if (timerTextComponent == null)
        {
            Debug.LogError("Timer Prefab is missing a TextMeshProUGUI component!");
            yield break;
        }

        while (timeRemaining > 0)
        {
            timerTextComponent.text = Mathf.Ceil(timeRemaining).ToString();
            yield return new WaitForSeconds(1f);
            timeRemaining--;
        }

        if (currentTimerText != null)
        {
            Destroy(currentTimerText);
            currentTimerText = null;
            Debug.Log("Timer UI destroyed.");
        }

        CheckTestResult();
        isTimerRunning = false;
        isCompleted = true;
    }

    private void CheckTestResult()
    {
        if (currentPatient == null)
        {
            Debug.LogError("No patient data available for testing.");
            return;
        }

        DiseaseInfo disease = DiseaseManager.Instance.GetDiseaseInfo(currentPatient.disease);
        if (disease == null)
        {
            Debug.LogError("Disease info not found for: " + currentPatient.disease);
            return;
        }

        bool isTestValid = disease.tests.Contains(testName);
        GameObject resultPrefab = isTestValid ? positivePrefab : negativePrefab;

        if (resultPrefab != null)
        {
            currentResultPrefab = Instantiate(resultPrefab, transform);
            currentResultPrefab.transform.SetParent(transform, false);
            RectTransform resultRect = currentResultPrefab.GetComponent<RectTransform>();
            resultRect.anchoredPosition = new Vector2(0, 3);
            Debug.Log("Test result displayed: " + (isTestValid ? "Positive" : "Negative"));
        }
        else
        {
            Debug.LogError("Result prefab is missing!");
        }
=======
    public void PerformTest(string testName, TestItem testItem)
    {
        if (activeTests.Contains(testItem))
        {
            Debug.Log($"[{testName}] Test already in progress.");
            return;
        }

        if (!testResultTexts.ContainsKey(testName))
        {
            Debug.LogWarning($"[DiagnosticsManager] No TMP field found for '{testName}'. Skipping test.");
            return;
        }

        if (TimerManager.Instance == null)
        {
            Debug.LogError("[DiagnosticsManager] TimerManager instance is missing!");
            return;
        }

        testResultTexts[testName].text = "Pending";

        bool isPositive = IsTestPositive(testName);
        Debug.Log($"[{testName}] Test started. Result: {(isPositive ? "Positive" : "Negative")}");

        activeTests.Add(testItem);
        TimerManager.Instance.StartTestTimer();
        StartCoroutine(HandleTestCompletion(testItem, testName, isPositive));
    }

    private IEnumerator HandleTestCompletion(TestItem testItem, string testName, bool isPositive)
    {
        yield return new WaitForSeconds(TimerManager.Instance.testDuration);

        testItem.MarkAsTested();

        if (testResultTexts.ContainsKey(testName))
        {
            testResultTexts[testName].text = isPositive ? "Positive" : "Negative";
        }

        Debug.Log($"[{testName}] Test completed. Final Result: {(isPositive ? "Positive" : "Negative")}");
        activeTests.Remove(testItem);
    }

    public bool IsOverDropZone(TestItem testItem)
    {
        if (testItem == null || dropZone == null)
        {
            return false;
        }

        return RectTransformUtility.RectangleContainsScreenPoint(dropZone, testItem.transform.position);
>>>>>>> Stashed changes
    }
}
