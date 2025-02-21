using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class DiagnosticsManager : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public GameObject checkMarkPrefab;
    public GameObject timerTextPrefab;
    public Transform patientArea;
    public float timerDuration = 5f;

    private RectTransform rectTransform;
    private Vector2 originalPosition;
    private bool isTimerRunning = false;
    private bool isCompleted = false;
    private GameObject currentTimerText;
    private GameObject currentCheckmark;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalPosition = rectTransform.anchoredPosition; // Save original UI position
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

        if (RectTransformUtility.RectangleContainsScreenPoint(patientArea.GetComponent<RectTransform>(), eventData.position))
        {
            rectTransform.anchoredPosition = originalPosition; // Snap back to start
            StartCoroutine(StartTimer()); // Start timer
        }
        else
        {
            rectTransform.anchoredPosition = originalPosition; // Reset position if not in target
        }
    }

    private IEnumerator StartTimer()
    {
        isTimerRunning = true;

        // Spawn Timer UI
        if (currentTimerText == null && timerTextPrefab != null)
        {
            currentTimerText = Instantiate(timerTextPrefab, transform);
            currentTimerText.transform.SetParent(transform, false);
            RectTransform timerRect = currentTimerText.GetComponent<RectTransform>();
            timerRect.anchoredPosition = new Vector2(0, 0);
        }

        float timeRemaining = timerDuration;
        TextMeshProUGUI timerTextComponent = currentTimerText.GetComponent<TextMeshProUGUI>();

        while (timeRemaining > 0)
        {
            if (timerTextComponent != null)
            {
                timerTextComponent.text = Mathf.Ceil(timeRemaining).ToString();
            }
            yield return new WaitForSeconds(1f);
            timeRemaining--;
        }

        if (currentTimerText != null) Destroy(currentTimerText); // Remove timer

        // Spawn Checkmark UI
        if (currentCheckmark == null && checkMarkPrefab != null)
        {
            currentCheckmark = Instantiate(checkMarkPrefab, transform);
            currentCheckmark.transform.SetParent(transform, false);
            RectTransform checkmarkRect = currentCheckmark.GetComponent<RectTransform>();
            checkmarkRect.anchoredPosition = new Vector2(0, 0);

            isTimerRunning = false;
            isCompleted = true;
        }
    }
}
