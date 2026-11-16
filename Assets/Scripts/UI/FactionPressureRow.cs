using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FactionPressureRow : MonoBehaviour
{
    [Tooltip("Inner object that gets shaken (not the layout-controlled root).")]
    public RectTransform content;
    public Image swatch;
    public Image factionIcon;
    public TextMeshProUGUI nameText;
    public SegmentedBar bar;
    public TextMeshProUGUI statusText;
    public Image warningIcon;
    public TextMeshProUGUI lostText;
    public CanvasGroup group;
    public TooltipTrigger tooltip;

    private FactionDefinition faction;
    private int lastPressure;
    private Vector2 origin;
    private Coroutine shakeCo, flashCo;

    public void Bind(FactionDefinition f)
    {
        faction = f;
        origin = content.anchoredPosition;
        nameText.text = f.displayName;
        if (swatch != null) swatch.color = f.color;
        if (factionIcon != null)
        {
            factionIcon.sprite = f.icon;
            factionIcon.enabled = f.icon != null;
            factionIcon.color = f.color;
        }
        if (tooltip != null) tooltip.Provider = TooltipContent;
        lastPressure = GameController.Instance.GetPressure(f);
        Refresh();
    }

    private (string, string, Color?) TooltipContent()
    {
        GameStrings s = GameStrings.Current;
        string body = string.Format(s.pressureTooltipFormat, faction.displayName, s.baseName);
        PerkDefinition next = PerkManager.Instance.NextToLose(faction);
        if (next != null) body += $"\n\nAt risk: <b>{next.displayName}</b>";
        int lost = PerkManager.Instance.LostCount(faction);
        if (lost > 0) body += $"\nAcquired so far: {lost}";
        return ($"{faction.displayName}  {s.pressureLabel}", body, faction.color);
    }

    public void OnPressureChanged(int pressure)
    {
        bool rose = pressure > lastPressure;
        lastPressure = pressure;
        Refresh();
        if (!rose) return;

        Image seg = bar != null ? bar.SegmentAt(Mathf.Min(pressure, faction.lockoutThreshold) - 1) : null;
        if (seg != null)
        {
            if (flashCo != null) StopCoroutine(flashCo);
            flashCo = UITween.Flash(this, seg, Color.white, seg.color, 0.4f);
        }
        if (shakeCo != null) StopCoroutine(shakeCo);
        shakeCo = UITween.Shake(this, content, origin, 6f, 0.3f);

        if (FloatingTextPool.Instance != null)
            FloatingTextPool.Instance.SpawnScreen("+1", faction.color, (Vector2)statusText.rectTransform.position, 0.8f);
    }

    public void OnRevoked()
    {
        Refresh();
        UITween.Punch(this, content, 0.1f, 0.35f);
    }

    public void Refresh()
    {
        GameController gc = GameController.Instance;
        PerkManager pm = PerkManager.Instance;
        UITheme t = UITheme.Current;
        GameStrings s = GameStrings.Current;

        int p = gc.GetPressure(faction);
        int max = Mathf.Max(1, faction.lockoutThreshold);
        bool locked = gc.IsLockedOut(faction);
        float pct = (float)p / max;
        PerkDefinition next = pm.NextToLose(faction);
        bool oneAway = p + 1 >= max;
        bool warn = !locked && p > 0 && (pct >= 0.6f || oneAway);

        bar.Set(Mathf.Min(p, max), max, locked ? t.disabled : faction.color);

        if (locked) statusText.text = s.lockedLabel;
        else if (oneAway && next != null) statusText.text = string.Format(s.nextHitRevokesFormat, next.displayName);
        else statusText.text = $"{p}/{max}";
        statusText.color = locked ? t.textMuted : (warn || (oneAway && next != null)) ? t.warning : t.textMuted;

        if (warningIcon != null) warningIcon.enabled = warn;
        if (lostText != null) lostText.text = new string('×', pm.LostCount(faction));
        if (group != null) group.alpha = locked ? 0.55f : 1f;
    }

    private void Update()
    {
        if (warningIcon != null && warningIcon.enabled)
        {
            Color c = UITheme.Current.warning;
            c.a = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 6f);
            warningIcon.color = c;
        }
    }
}
