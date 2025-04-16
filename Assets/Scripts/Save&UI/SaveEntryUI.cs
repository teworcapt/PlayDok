using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SaveEntryUI : MonoBehaviour
{
    [SerializeField] private TMP_Text dayText;
    [SerializeField] private TMP_Text creditsText;
    [SerializeField] public Button loadButton;

    public void SetData(string dayOfWeek, int credits, int penalties, int day)
    {
        if (dayText) dayText.text = dayOfWeek;
        if (creditsText) creditsText.text = credits.ToString();
        SetupLoadButton(dayOfWeek);
    }

    private void SetupLoadButton(string dayString)
    {
        if (loadButton == null)
            return;
        string savePath = SaveManager.GetSaveFilePath(dayString);
        bool saveExists = System.IO.File.Exists(savePath);
        loadButton.interactable = saveExists;
        loadButton.onClick.RemoveAllListeners();
        if (!saveExists)
            return;
        loadButton.onClick.AddListener(() =>
        {
            Debug.Log($"Button clicked for day: {dayString}");
            LoadSaveManager.Instance.LoadSelectedDay(dayString);
        });
    }

    public void SetLoadButtonInteractable(bool isInteractable)
    {
        if (loadButton != null)
            loadButton.interactable = isInteractable;
    }
}
