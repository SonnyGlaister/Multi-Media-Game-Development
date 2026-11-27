using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthView : MonoBehaviour
{
    [Tooltip("Inner object that shakes (not a layout-controlled root).")]
    public RectTransform content;
    public SegmentedBar bar;
    public TextMeshProUGUI valueText;
    public TooltipTrigger tooltip;

    [Header("Breach feedback")]
    [Tooltip("Full-screen Image with the vignette sprite, Raycast Target OFF.")]
    public Image damageVignette;
    public float vignetteDuration = 0.6f;

    private GameController gc;

    private void Start()
    {
        gc = GameController.Instance;
        GameStrings s = GameStrings.Current;

        if (damageVignette != null)
        {
            damageVignette.raycastTarget = false;
            damageVignette.color = Color.clear;
        }

        if (tooltip != null) tooltip.Provider = () =>
            (s.baseName, $"If this reaches 0, the AI takes over.\n{gc.BaseHealth} / {gc.MaxBaseHealth}", (Color?)UITheme.Current.health);

        gc.BaseHealthChanged += OnHealth;
        gc.BaseHit += OnBaseHit;
        OnHealth(gc.BaseHealth, gc.MaxBaseHealth);
    }

    private void OnDestroy()
    {
        if (gc == null) return;
        gc.BaseHealthChanged -= OnHealth;
        gc.BaseHit -= OnBaseHit;
    }

    private void OnHealth(int current, int max)
    {
        UITheme t = UITheme.Current;
        float k = max > 0 ? (float)current / max : 0f;
        Color c = k > 0.5f ? t.health : k > 0.25f ? t.warning : t.danger;
        if (bar != null) bar.Set(current, max, c);
        if (valueText != null) valueText.text = $"{current}/{max}";
    }

    private void OnBaseHit(EnemyDefinition who, int damage)
    {
        UITheme t = UITheme.Current;
        GameStrings s = GameStrings.Current;

        content.DOKill(true);
        content.DOShakeAnchorPos(0.35f, 10f, 20).SetUpdate(true);

        if (damageVignette != null)
        {
            Color tint = who != null && who.faction != null ? Color.Lerp(t.danger, who.faction.color, 0.35f) : t.danger;
            tint.a = 0.55f;
            damageVignette.DOKill();
            damageVignette.color = tint;
            damageVignette.DOFade(0f, vignetteDuration).SetUpdate(true);
        }

        gc.ShowMessage(string.Format(s.baseHitFormat, who != null ? who.displayName : "?", s.baseName, damage), MessageType.Danger);
    }
}
