using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string title;
    [TextArea] public string text;
    public Func<(string title, string body, Color? accent)> Provider;

    private bool hovering;
    private float refreshTimer;

    public void OnPointerEnter(PointerEventData eventData)
    {
        hovering = true;
        ShowNow();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovering = false;
        TooltipManager.HideFor(this);
    }

    private void OnDisable()
    {
        if (hovering) TooltipManager.HideFor(this);
        hovering = false;
    }

    private void Update()
    {
        if (!hovering || Provider == null) return;
        refreshTimer -= Time.unscaledDeltaTime;
        if (refreshTimer <= 0f) ShowNow();
    }

    private void ShowNow()
    {
        refreshTimer = 0.25f;
        if (TooltipManager.Instance == null) return;
        if (Provider != null)
        {
            var c = Provider();
            TooltipManager.Instance.Show(this, c.title, c.body, c.accent);
        }
        else
        {
            TooltipManager.Instance.Show(this, title, text);
        }
    }
}
