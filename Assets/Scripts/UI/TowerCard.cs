using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TowerCard : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Image background;
    public Image icon;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI costText;
    public TextMeshProUGUI nextCostText;
    public TextMeshProUGUI hotkeyText;
    public CanvasGroup group;
    public TooltipTrigger tooltip;

    public TowerController Prefab { get; private set; }

    private bool dragging;
    private Color normalColor;

    public void Setup(TowerController prefab, int hotkey)
    {
        normalColor = background.color;
        Prefab = prefab;
        icon.sprite = prefab.icon;
        icon.enabled = prefab.icon != null;
        nameText.text = prefab.displayName;
        hotkeyText.text = hotkey <= 9 ? hotkey.ToString() : "";
        if (tooltip != null) tooltip.Provider = () => (prefab.displayName, TooltipBody(), (Color?)null);
    }

    private void Update()
    {
        TowerPlacer placer = TowerPlacer.Instance;
        UITheme t = UITheme.Current;

        int cost = placer.CostOf(Prefab);
        int next = placer.NextCostOf(Prefab);
        bool afford = GameController.Instance.CanAfford(cost);

        costText.text = cost.ToString();
        costText.color = afford ? t.skillPoints : t.danger;
        nextCostText.text = next != cost ? string.Format(GameStrings.Current.nextCostFormat, next) : "";
        group.alpha = afford ? 1f : 0.55f;

        bool active = placer.IsPlacing && placer.Pending == Prefab;
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
        placer.BeginPlacement(Prefab, TowerPlacer.Mode.Clicking);
    }

    public void OnPointerClick(PointerEventData e)
    {
        if (dragging || e.button != PointerEventData.InputButton.Left) return;
        TowerPlacer placer = TowerPlacer.Instance;
        if (placer.IsPlacing && placer.Pending == Prefab) placer.Cancel();
        else StartClickPlacement();
    }

    public void OnBeginDrag(PointerEventData e)
    {
        TowerPlacer placer = TowerPlacer.Instance;
        if (!placer.CanPlace()) return;
        dragging = true;
        placer.BeginPlacement(Prefab, TowerPlacer.Mode.Dragging);
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
        GameStrings s = GameStrings.Current;
        string muted = UITheme.Hex(UITheme.Current.textMuted);

        string body = string.IsNullOrEmpty(Prefab.description) ? "" : Prefab.description + "\n\n";
        body += Prefab.StatsText();
        body += $"\n\nCost {placer.CostOf(Prefab)}  (next {placer.NextCostOf(Prefab)})";
        body += $"\n<size=80%><color=#{muted}>{s.towerHint}</color></size>";
        return body;
    }
}
