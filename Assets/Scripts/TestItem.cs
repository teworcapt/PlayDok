using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class TestItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
    [Header("Test Item Settings")]
    public string testName;
    public Image testImage;
    public Sprite testedSprite;

    [Header("Tooltip Settings")]
    public GameObject tooltipPanel;
    public TextMeshProUGUI tooltipText;

    private Sprite defaultSprite;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector3 originalPosition;
    private Transform originalParent;
    private Canvas canvas;
    private bool isTested = false;
    private bool isTestPositive = false;

    /* -------------------- Public Properties -------------------- */
    public bool IsTested => isTested;
    public bool IsTestPositive => isTestPositive;

    /* -------------------- Initialization -------------------- */
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        originalPosition = rectTransform.anchoredPosition;
        originalParent = transform.parent;
        canvas = GetComponentInParent<Canvas>();
        if (testImage != null)
        {
            defaultSprite = testImage.sprite;
        }
        HideTooltip();
    }

    /* -------------------- Drag Handling -------------------- */
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isTested) return;
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
        transform.SetParent(canvas.transform);
        HideTooltip();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isTested) return;
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        transform.SetParent(originalParent);
        if (DiagnosticsManager.Instance != null && DiagnosticsManager.Instance.IsOverDropZone(this))
        {
            DiagnosticsManager.Instance.PerformTest(testName, this);
        }
        ResetPosition();
    }

    /* -------------------- Tooltip Handling -------------------- */
    public void OnPointerEnter(PointerEventData eventData)
    {
        ShowTooltip(eventData.position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HideTooltip();
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        if (tooltipPanel != null && tooltipPanel.activeSelf)
        {
            UpdateTooltipPosition(eventData.position);
        }
    }

    private void ShowTooltip(Vector3 screenPosition)
    {
        if (tooltipPanel != null && tooltipText != null)
        {
            tooltipText.text = testName;

            // First position the tooltip before showing it
            UpdateTooltipPosition(screenPosition);

            tooltipPanel.SetActive(true);
        }
    }

    private void HideTooltip()
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
    }

    private void UpdateTooltipPosition(Vector3 screenPosition)
    {
        if (tooltipPanel == null) return;

        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        RectTransform tooltipRect = tooltipPanel.GetComponent<RectTransform>();
        Vector2 localPoint;

        // Add offset for better visibility (not too close to cursor)
        Vector3 offsetPosition = screenPosition + new Vector3(5f, -5f, 0);

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            offsetPosition,
            canvas.worldCamera,
            out localPoint))
        {
            // Get tooltip and canvas dimensions to ensure it stays on screen
            Vector2 tooltipSize = tooltipRect.rect.size;
            Vector2 canvasSize = canvasRect.rect.size;

            // Adjust for right edge
            float rightEdge = canvasSize.x / 1.7f - localPoint.x - tooltipSize.x;
            if (rightEdge < 0)
            {
                localPoint.x += rightEdge - 2f;
            }

            // Adjust for bottom edge
            float bottomEdge = localPoint.y + canvasSize.y / 1 - tooltipSize.y;
            if (bottomEdge < 0)
            {
                localPoint.y -= bottomEdge - 1f;
            }

            if (localPoint.y > canvasSize.y / 2 - 1f)
            {
                localPoint.y = canvasSize.y / 2 - 1f;
            }

            if (localPoint.x < -canvasSize.x / 2 + 1f)
            {
                localPoint.x = -canvasSize.x / 2 + 1f;
            }

            tooltipRect.anchoredPosition = localPoint;
        }
    }

    /* -------------------- Test Completion -------------------- */
    public void MarkAsTested()
    {
        isTested = true;
        if (testImage != null && testedSprite != null)
        {
            testImage.sprite = testedSprite;
        }
    }

    public void SetTestResult(bool isPositive)
    {
        isTestPositive = isPositive;
        Debug.Log($"Test {testName} result set to {(isPositive ? "Positive" : "Negative")}");
    }

    /* -------------------- Reset Functions -------------------- */
    public void ResetPosition()
    {
        rectTransform.anchoredPosition = originalPosition;
    }

    public void ResetState()
    {
        isTested = false;
        isTestPositive = false;
        ResetPosition();
        if (testImage != null && defaultSprite != null)
        {
            testImage.sprite = defaultSprite;
        }
    }
}