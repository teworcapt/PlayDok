using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaveEntryUI : MonoBehaviour
{
    /* -------------------- UI References -------------------- */
    [Header("UI References")]
    [SerializeField] private TMP_Text dayText;
    [SerializeField] private TMP_Text creditsText;
    [SerializeField] private TMP_Text penaltiesText;
    [SerializeField] private Button loadButton;

    /* -------------------- Public Methods -------------------- */
    public void SetData(string dayOfWeek, int credits, int penalties, int day)
    {
        if (dayText) dayText.text = dayOfWeek;
        if (creditsText) creditsText.text = $"{credits}";
        if (penaltiesText) penaltiesText.text = $"{penalties}";

        string dayString = GetDayOfWeekString(day);
        SetupLoadButton(dayString);
    }

    /* -------------------- Private Methods -------------------- */
    private void SetupLoadButton(string dayString)
    {
        if (loadButton == null) return;

        loadButton.onClick.RemoveAllListeners();
        loadButton.onClick.AddListener(() =>
        {
            Debug.Log($"Button clicked for day: {dayString}");
            LoadSaveManager.Instance?.LoadSelectedDay(dayString);
        });

        Debug.Log("Button listener added.");
    }

    private string GetDayOfWeekString(int dayIndex)
    {
        string[] daysOfWeek = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
        return (dayIndex >= 1 && dayIndex <= 7) ? daysOfWeek[dayIndex - 1] : "Invalid Day";
    }
}
