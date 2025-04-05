using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class TestItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public string testName;
    public Image testImage;
    public Sprite testedSprite;
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
    }

    /* -------------------- Drag Handling -------------------- */
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isTested) return;
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
        transform.SetParent(canvas.transform);
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
        if (DiagnosticsManager.Instance.IsOverDropZone(this))
        {
            DiagnosticsManager.Instance.PerformTest(testName, this);
        }
        ResetPosition();
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