using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class DiagnosticsManager : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public string testName;
    public GameObject positivePrefab;
    public GameObject negativePrefab;
    public GameObject timerTextPrefab;
    public Transform patientDropzone; // Updated from patientArea
    public float timerDuration = 5f;

    private RectTransform rectTransform;
    private Vector2 originalPosition;
    private bool isTimerRunning = false;
    private bool isCompleted = false;
    private GameObject currentTimerText;
    private PatientData currentPatient;
    private PatientDropzone dropzoneScript; // Reference to new PatientDropzone

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalPosition = rectTransform.anchoredPosition;

        // Get the PatientDropzone script from the dropzone
        if (patientDropzone != null)
        {
            dropzoneScript = patientDropzone.GetComponent<PatientDropzone>();
        }
        else
        {
            Debug.LogError("❌ PatientDropzone is NOT assigned in the Inspector!");
        }
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

        Debug.Log("Patient Dropzone: " + patientDropzone);
        Debug.Log("PatientDropzone Script Reference: " + dropzoneScript);

        if (RectTransformUtility.RectangleContainsScreenPoint(patientDropzone.GetComponent<RectTransform>(), eventData.position))
        {
            Debug.Log("✅ Dropped on patient! Starting timer...");
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
                Debug.LogError("❌ Current patient is NULL! Timer will not start.");
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
            Debug.LogError("❌ TimerTextPrefab is NOT assigned in the Inspector!");
            yield break;
        }

        if (currentTimerText == null)
        {
            Debug.Log("✅ Instantiating Timer Prefab...");
            currentTimerText = Instantiate(timerTextPrefab, transform.parent);
            RectTransform timerRect = currentTimerText.GetComponent<RectTransform>();
            if (timerRect != null)
            {
                timerRect.anchoredPosition = new Vector2(0, -100);
            }
            else
            {
                Debug.LogError("❌ Timer Prefab is missing a RectTransform!");
            }
        }

        float timeRemaining = timerDuration;
        TextMeshProUGUI timerTextComponent = currentTimerText.GetComponent<TextMeshProUGUI>();

        if (timerTextComponent == null)
        {
            Debug.LogError("❌ Timer Prefab is missing a TextMeshProUGUI component!");
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
            GameObject result = Instantiate(resultPrefab, transform);
            result.transform.SetParent(transform, false);
            RectTransform resultRect = result.GetComponent<RectTransform>();
            resultRect.anchoredPosition = new Vector2(0, 3);
            Debug.Log("Test result displayed: " + (isTestValid ? "Positive" : "Negative"));
        }
        else
        {
            Debug.LogError("Result prefab is missing!");
        }
    }
}
