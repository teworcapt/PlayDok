using UnityEngine;
using UnityEngine.EventSystems;

public class MonitorPanelToggle : MonoBehaviour
{
    public GameObject panel;

    private bool isPanelOpen = false;

    void Update()
    {
        if (isPanelOpen && Input.GetMouseButtonDown(0))
        {
            if (!IsPointerOverUIObject())
            {
                TogglePanel(false); // Close the panel
            }
        }
    }

    public void TogglePanel()
    {
        isPanelOpen = !isPanelOpen;
        panel.SetActive(isPanelOpen);
    }

    public void TogglePanel(bool state)
    {
        isPanelOpen = state;
        panel.SetActive(state);
    }

    private bool IsPointerOverUIObject()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var result in results)
        {
            if (result.gameObject == gameObject || result.gameObject == panel)
            {
                return true;
            }
        }

        return false;
    }
}
