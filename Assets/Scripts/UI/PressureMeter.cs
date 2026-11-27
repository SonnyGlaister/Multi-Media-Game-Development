using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// AI pressure bar
public class PressureMeter : MonoBehaviour
{
    [Tooltip("Inner object that gets shaken (not the layout-controlled root).")]
    public RectTransform content;
    [Tooltip("Image with Image Type = Filled, Fill Method = Horizontal.")]
    public Image fill;
    [Tooltip("Optional: shows 'AI Influence'.")]
    public TextMeshProUGUI label;
    public TextMeshProUGUI statusText;
    [Tooltip("Optional: pulses when a skill is about to be taken.")]
    public Image warningIcon;
    public TooltipTrigger tooltip;
    [Tooltip("Optional thin line inside the bar, showing where it drops back to.")]
    public RectTransform resetMarker;
    [Range(0f, 1f)] public float dangerAt = 0.8f;

    private GameController gc;
    private PerkManager pm;

    private void Start()
    {
        gc = GameController.Instance;
        pm = PerkManager.Instance;

        if (label != null) label.text = GameStrings.Current.pressureLabel;
        if (tooltip != null) tooltip.Provider = TooltipContent;
        if (resetMarker != null)
        {
            resetMarker.anchorMin = new Vector2(gc.pressureAfterRevoke, 0f);
            resetMarker.anchorMax = new Vector2(gc.pressureAfterRevoke, 1f);
            resetMarker.anchoredPosition = Vector2.zero;
        }
        fill.fillAmount = 0f;

        gc.PressureAdded += OnPressureAdded;
        pm.PerkRevoked += OnPerkRevoked;
    }

    private void OnDestroy()
    {
        if (gc != null) gc.PressureAdded -= OnPressureAdded;
        if (pm != null) pm.PerkRevoked -= OnPerkRevoked;
    }

    private void Update()
    {
        UITheme t = UITheme.Current;
        GameStrings s = GameStrings.Current;
        float k = gc.Pressure01;
        PerkDefinition next = pm.NextToLose;
        bool danger = k >= dangerAt;

        fill.fillAmount = Mathf.MoveTowards(fill.fillAmount, k, Time.unscaledDeltaTime * 2f); // slide, don't jump
        fill.color = danger ? t.danger : k >= 0.5f ? t.warning : t.info;

        statusText.text = next != null ? string.Format(s.atRiskFormat, next.displayName) : s.nothingAtRisk;
        statusText.color = danger && next != null ? t.warning : t.textMuted;

        if (warningIcon != null)
        {
            warningIcon.enabled = danger && next != null;
            Color c = t.warning;
            c.a = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 6f);
            warningIcon.color = c;
        }
    }

    private void OnPressureAdded(float amount)
    {
        content.DOKill(true);
        content.DOShakeAnchorPos(0.3f, 6f, 20).SetUpdate(true);
        if (FloatingTextPool.Instance != null)
            FloatingTextPool.Instance.SpawnScreen($"+{amount:0.#}", UITheme.Current.danger,
                (Vector2)statusText.rectTransform.position, 0.8f);
    }

    private void OnPerkRevoked(PerkDefinition perk)
    {
        content.DOKill(true);
        content.DOPunchScale(Vector3.one * 0.12f, 0.35f, 1, 0f).SetUpdate(true);
        if (FloatingTextPool.Instance != null)
            FloatingTextPool.Instance.SpawnScreen($"-{perk.displayName}", UITheme.Current.danger,
                (Vector2)statusText.rectTransform.position, 1f);
    }

    private (string, string, Color?) TooltipContent()
    {
        GameStrings s = GameStrings.Current;
        string body = string.Format(s.pressureTooltip, s.baseName, gc.pressureAfterRevoke);
        body += $"\n\n{gc.Pressure:0.#} / {gc.pressureLimit:0.#}";
        PerkDefinition next = pm.NextToLose;
        if (next != null) body += "\n" + string.Format(s.atRiskFormat, $"<b>{next.displayName}</b>");
        if (pm.LostCount > 0) body += $"\nTaken so far: {pm.LostCount}";
        return (s.pressureLabel, body, UITheme.Current.danger);
    }
}
