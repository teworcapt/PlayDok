using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;

public enum NotificationType
{
    Default,
    TimeBoost,
    Penalty,
    CorrectDiagnosis,
    IncorrectDiagnosis
}

public class UINotification : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Optional Background")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Sprite redBackgroundSprite;
    [SerializeField] private Sprite greenBackgroundSprite;
    [SerializeField] private Sprite defaultBackgroundSprite;

    [Header("Default Animation Settings")]
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float moveUpDistance = 50f;
    [SerializeField] private float displayDuration = 2f;

    public void Play(string message, Color color, NotificationType type = NotificationType.Default)
    {
        messageText.text = message;
        messageText.color = color;
        canvasGroup.alpha = 0f;

        if (backgroundImage != null)
        {
            backgroundImage.type = Image.Type.Sliced;
            backgroundImage.fillCenter = true;
            backgroundImage.pixelsPerUnitMultiplier = 10f;

            Debug.Log($"[UINotification] Background image settings applied: " +
                      $"Type = {backgroundImage.type}, FillCenter = {backgroundImage.fillCenter}, " +
                      $"PixelsPerUnit = {backgroundImage.pixelsPerUnitMultiplier}");

            if (type == NotificationType.TimeBoost || type == NotificationType.Penalty)
            {
                backgroundImage.enabled = false;
            }
            else if (type == NotificationType.IncorrectDiagnosis)
            {
                backgroundImage.enabled = true;
                backgroundImage.sprite = redBackgroundSprite;
            }
            else if (type == NotificationType.CorrectDiagnosis)
            {
                backgroundImage.enabled = true;
                backgroundImage.sprite = greenBackgroundSprite;
            }
            else
            {
                backgroundImage.enabled = true;
                backgroundImage.sprite = defaultBackgroundSprite;
            }
        }
        else
        {
            Debug.LogWarning("[UINotification] No background image assigned to this notification.");
        }


        float finalFadeDuration = fadeDuration;
        float finalDisplayDuration = displayDuration;
        float finalMoveUpDistance = moveUpDistance;

        switch (type)
        {
            case NotificationType.TimeBoost:
                finalFadeDuration = 0.3f;
                finalDisplayDuration = 2f;
                finalMoveUpDistance = 60f;
                break;
            case NotificationType.Penalty:
                finalFadeDuration = 0.5f;
                finalDisplayDuration = 2.5f;
                finalMoveUpDistance = 40f;
                break;
            case NotificationType.CorrectDiagnosis:
                finalFadeDuration = 0.4f;
                finalDisplayDuration = 2.2f;
                finalMoveUpDistance = 55f;
                break;
            case NotificationType.IncorrectDiagnosis:
                finalFadeDuration = 0.4f;
                finalDisplayDuration = 2.2f;
                finalMoveUpDistance = 45f;
                break;
            default:
                break;
        }

        Vector3 startPos = transform.localPosition;
        transform.localPosition = new Vector3(startPos.x, startPos.y - finalMoveUpDistance, startPos.z);

        Sequence seq = DOTween.Sequence();
        seq.Append(canvasGroup.DOFade(1f, finalFadeDuration));
        seq.Join(transform.DOLocalMoveY(startPos.y, finalDisplayDuration).SetEase(Ease.OutQuad));
        seq.Append(canvasGroup.DOFade(0f, finalFadeDuration));
        seq.OnComplete(() => Destroy(gameObject));
    }
}
