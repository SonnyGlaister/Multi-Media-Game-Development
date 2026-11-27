using DG.Tweening;
using TMPro;
using UnityEngine;

public class SkillPointsView : MonoBehaviour
{
    [Tooltip("Inner object that pops/shakes (not a layout-controlled root).")]
    public RectTransform content;
    public TextMeshProUGUI valueText;
    public TooltipTrigger tooltip;
    public float countDuration = 0.35f;

    private GameController gc;
    private int lastValue;
    private int shownValue;
    private Tween countTween;

    private void Start()
    {
        gc = GameController.Instance;
        GameStrings s = GameStrings.Current;

        if (tooltip != null) tooltip.Provider = () =>
            (s.skillPointsName, "Spend on towers and skills.\nEarned by stopping AI models.", (Color?)UITheme.Current.skillPoints);

        gc.SkillPointsChanged += OnSkillPoints;
        gc.PurchaseFailed += OnPurchaseFailed;

        lastValue = shownValue = gc.SkillPoints;
        valueText.text = shownValue.ToString();
    }

    private void OnDestroy()
    {
        if (gc == null) return;
        gc.SkillPointsChanged -= OnSkillPoints;
        gc.PurchaseFailed -= OnPurchaseFailed;
    }

    private void OnSkillPoints(int value)
    {
        int delta = value - lastValue;
        lastValue = value;

        countTween?.Kill();
        countTween = DOTween.To(() => shownValue, v =>
        {
            shownValue = v;
            valueText.text = v.ToString();
        }, value, countDuration).SetUpdate(true).SetLink(gameObject);

        if (delta > 0)
        {
            content.DOKill(true);
            content.DOPunchScale(Vector3.one * 0.08f, 0.2f, 1, 0f).SetUpdate(true);
            if (FloatingTextPool.Instance != null)
                FloatingTextPool.Instance.SpawnScreen($"+{delta}", UITheme.Current.skillPoints,
                    (Vector2)valueText.rectTransform.position + new Vector2(0f, 30f), 0.8f);
        }
    }

    private void OnPurchaseFailed(int cost)
    {
        UITheme t = UITheme.Current;

        valueText.DOKill();
        valueText.color = t.danger;
        valueText.DOColor(t.textPrimary, 0.4f).SetUpdate(true);

        content.DOKill(true);
        content.DOShakeAnchorPos(0.25f, 6f, 20).SetUpdate(true);
    }
}
