using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TowerCard : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Image background;
    [Tooltip("Optional strip/border tinted with the tower faction's colour.")]
    public Image accent;
    public Image icon;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI costText;
    public TextMeshProUGUI nextCostText;
    public TextMeshProUGUI hotkeyText;
    public CanvasGroup group;
    public TooltipTrigger tooltip;

    public TowerDefinition Tower { get; private set; }

    private bool dragging;
    private Color normalColor;
    private int shownTier;

    private int CurrentTier => PerkManager.Instance != null ? PerkManager.Instance.TierOf(Tower) : 1;

    public void Setup(TowerDefinition tower, int hotkey)
    {
        normalColor = background.color;
        Tower = tower;
        hotkeyText.text = hotkey <= 9 ? hotkey.ToString() : "";
        if (accent != null) accent.color = tower.color;
        if (tooltip != null) tooltip.Provider = () => (Tower.GetTier(CurrentTier).name, TooltipBody(), (Color?)Tower.color);
        ShowTier();
    }

    private void ShowTier()
    {
        shownTier = CurrentTier;
        TowerTier tier = Tower.GetTier(shownTier);
        icon.sprite = tier.icon;
        icon.enabled = tier.icon != null;
        nameText.text = tier.name;
    }

    private void Update()
    {
        TowerPlacer placer = TowerPlacer.Instance;
        UITheme t = UITheme.Current;

        if (CurrentTier != shownTier)
        {
            ShowTier();
            transform.DOKill(true);
            transform.DOPunchScale(Vector3.one * 0.12f, 0.3f, 1, 0f).SetUpdate(true);
        }

        int cost = placer.CostOf(Tower);
        int next = placer.NextCostOf(Tower);
        bool afford = GameController.Instance.CanAfford(cost);

        costText.text = cost.ToString();
        costText.color = afford ? t.skillPoints : t.danger;
        nextCostText.text = next != cost ? string.Format(GameStrings.Current.nextCostFormat, next) : "";
        group.alpha = afford ? 1f : 0.55f;

        bool active = placer.IsPlacing && placer.Pending == Tower;
        background.color = active ? Color.Lerp(normalColor, t.info, 0.45f) : normalColor;
    }

    public void StartClickPlacement()
    {
        TowerPlacer placer = TowerPlacer.Instance;
        if (!placer.CanPlace())
        {
            if (!GameController.Instance.HasGameStarted)
                GameController.Instance.ShowMessage(GameStrings.Current.startPrompt);
            return;
        }
        placer.BeginPlacement(Tower, TowerPlacer.Mode.Clicking);
    }

    public void OnPointerClick(PointerEventData e)
    {
        if (dragging || e.button != PointerEventData.InputButton.Left) return;
        TowerPlacer placer = TowerPlacer.Instance;
        if (placer.IsPlacing && placer.Pending == Tower) placer.Cancel();
        else StartClickPlacement();
    }

    public void OnBeginDrag(PointerEventData e)
    {
        TowerPlacer placer = TowerPlacer.Instance;
        if (!placer.CanPlace()) return;
        dragging = true;
        placer.BeginPlacement(Tower, TowerPlacer.Mode.Dragging);
        placer.Drag(e.position);
    }

    public void OnDrag(PointerEventData e)
    {
        if (dragging) TowerPlacer.Instance.Drag(e.position);
    }

    public void OnEndDrag(PointerEventData e)
    {
        if (!dragging) return;
        dragging = false;
        TowerPlacer.Instance.EndDrag(e.position);
    }

    private string TooltipBody()
    {
        TowerPlacer placer = TowerPlacer.Instance;
        PerkManager pm = PerkManager.Instance;
        GameStrings s = GameStrings.Current;
        string muted = UITheme.Hex(UITheme.Current.textMuted);

        int tier = CurrentTier;
        TowerTier data = Tower.GetTier(tier);
        TowerStats stats = pm != null ? pm.StatsFor(Tower, tier) : data.stats;

        string body = $"<color=#{UITheme.Hex(Tower.color)}>{Tower.displayName}  ·  {string.Format(s.tierFormat, tier)}</color>\n";
        if (!string.IsNullOrEmpty(data.description)) body += data.description + "\n";
        body += "\n" + TowerStats.Describe(data.stats, stats);
        body += $"\n\nCost {placer.CostOf(Tower)}  (next {placer.NextCostOf(Tower)})";
        body += $"\n<size=80%><color=#{muted}>{s.towerHint}</color></size>";
        return body;
    }
}
