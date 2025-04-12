using TMPro;
using UnityEngine;

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instance { get; private set; }

    [Header("Notification Prefab Setup")]
    [SerializeField] private GameObject notificationPrefab;

    [Header("Notification Parent Transforms")]
    [SerializeField] private Transform defaultNotificationParent;
    [SerializeField] private Transform timerNotificationParent;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void ShowNotification(string message, Color color, NotificationType type = NotificationType.Default)
    {
        if (notificationPrefab == null)
        {
            Debug.LogWarning("Notification prefab not assigned.");
            return;
        }

        Transform parentTransform = defaultNotificationParent;
        switch (type)
        {
            case NotificationType.TimeBoost:
            case NotificationType.Penalty:
                parentTransform = timerNotificationParent != null ? timerNotificationParent : defaultNotificationParent;
                break;
            default:
                parentTransform = defaultNotificationParent;
                break;
        }

        GameObject notifGO = Instantiate(notificationPrefab, parentTransform);
        UINotification uiNotif = notifGO.GetComponent<UINotification>();

        if (uiNotif != null)
        {
            uiNotif.Play(message, color, type);
        }
        else
        {
            TMP_Text notificationText = notifGO.GetComponentInChildren<TMP_Text>();
            if (notificationText != null)
            {
                notificationText.text = message;
                notificationText.color = color;
            }
        }

        Debug.Log($"Notification Spawned: {type} - {message}");
    }
}
