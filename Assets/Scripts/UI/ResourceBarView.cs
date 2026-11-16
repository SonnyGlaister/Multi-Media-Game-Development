using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResourceBarView : MonoBehaviour
{
    [Header("HQ health")]
    public RectTransform healthGroup;
    public SegmentedBar healthBar;
    public TextMeshProUGUI healthText;
    public TooltipTrigger healthTooltip;

    [Header("Skill Points")]
    public RectTransform skillPointsGroup;
    public TextMeshProUGUI skillPointsText;
    public TooltipTrigger skillPointsTooltip;
    public float countDuration = 0.35f;

    [Header("Feedback")]
    [Tooltip("Full-screen Image, Raycast Target OFF. Sprite is generated if empty.")]
    public Image damageVignette;
    public float vignetteDuration = 0.6f;

    private GameController gc;
    private int lastSkillPoints;
    private int shownSkillPoints;
    private Coroutine countCo, vignetteCo, healthShakeCo, skillPointsShakeCo, skillPointsFlashCo;
    private Vector2 healthOrigin, skillPointsOrigin;

    private void Start()
    {
        gc = GameController.Instance;
        UITheme t = UITheme.Current;
        GameStrings s = GameStrings.Current;

        healthOrigin = healthGroup.anchoredPosition;
        skillPointsOrigin = skillPointsGroup.anchoredPosition;

        if (damageVignette != null)
        {
            if (damageVignette.sprite == null) damageVignette.sprite = RuntimeSprites.Vignette;
            damageVignette.raycastTarget = false;
            damageVignette.color = Color.clear;
        }

        if (healthTooltip != null) healthTooltip.Provider = () =>
            (s.baseName, $"If this reaches 0, the AI takes over.\n{gc.BaseHealth} / {gc.MaxBaseHealth}", (Color?)t.health);
        if (skillPointsTooltip != null) skillPointsTooltip.Provider = () =>
            (s.skillPointsName, $"Spend on defences and research.\nEarned by stopping AI models.", (Color?)t.skillPoints);

        gc.SkillPointsChanged += OnSkillPoints;
        gc.PurchaseFailed += OnPurchaseFailed;
        gc.BaseHealthChanged += OnHealth;
        gc.BaseHit += OnBaseHit;

        lastSkillPoints = shownSkillPoints = gc.SkillPoints;
        skillPointsText.text = shownSkillPoints.ToString();
        OnHealth(gc.BaseHealth, gc.MaxBaseHealth);
    }

    private void OnDestroy()
    {
        if (gc == null) return;
        gc.SkillPointsChanged -= OnSkillPoints;
        gc.PurchaseFailed -= OnPurchaseFailed;
        gc.BaseHealthChanged -= OnHealth;
        gc.BaseHit -= OnBaseHit;
    }

    private void OnSkillPoints(int value)
    {
        int delta = value - lastSkillPoints;
        lastSkillPoints = value;

        if (countCo != null) StopCoroutine(countCo);
        countCo = UITween.CountTo(this, shownSkillPoints, value, countDuration, v =>
        {
            shownSkillPoints = v;
            skillPointsText.text = v.ToString();
        });

        if (delta > 0)
        {
            UITween.Punch(this, skillPointsGroup, 0.08f, 0.2f);
            if (FloatingTextPool.Instance != null)
                FloatingTextPool.Instance.SpawnScreen($"+{delta}", UITheme.Current.skillPoints,
                    (Vector2)skillPointsText.rectTransform.position + new Vector2(0f, -30f), 0.8f);
        }
    }

    private void OnPurchaseFailed(int cost)
    {
        UITheme t = UITheme.Current;
        if (skillPointsFlashCo != null) StopCoroutine(skillPointsFlashCo);
        skillPointsFlashCo = UITween.Flash(this, skillPointsText, t.danger, t.textPrimary, 0.4f);
        if (skillPointsShakeCo != null) StopCoroutine(skillPointsShakeCo);
        skillPointsShakeCo = UITween.Shake(this, skillPointsGroup, skillPointsOrigin, 6f, 0.25f);
    }

    private void OnHealth(int current, int max)
    {
        UITheme t = UITheme.Current;
        float k = max > 0 ? (float)current / max : 0f;
        Color c = k > 0.5f ? t.health : k > 0.25f ? t.warning : t.danger;
        if (healthBar != null) healthBar.Set(current, max, c);
        if (healthText != null) healthText.text = $"{current}/{max}";
    }

    private void OnBaseHit(EnemyDefinition who, int damage)
    {
        UITheme t = UITheme.Current;
        GameStrings s = GameStrings.Current;

        if (healthShakeCo != null) StopCoroutine(healthShakeCo);
        healthShakeCo = UITween.Shake(this, healthGroup, healthOrigin, 10f, 0.35f);

        if (damageVignette != null)
        {
            Color tint = who != null && who.faction != null ? Color.Lerp(t.danger, who.faction.color, 0.35f) : t.danger;
            tint.a = 0.55f;
            Color clear = tint; clear.a = 0f;
            if (vignetteCo != null) StopCoroutine(vignetteCo);
            vignetteCo = UITween.Flash(this, damageVignette, tint, clear, vignetteDuration);
        }

        gc.ShowMessage(string.Format(s.baseHitFormat, who != null ? who.displayName : "?", s.baseName, damage), MessageType.Danger);
    }
}
