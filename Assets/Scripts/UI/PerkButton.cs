using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PerkButton : MonoBehaviour
{
    public Button button;
    public Image background;
    public Image overlay;
    public Sprite ownedSprite;
    public Sprite lockedSprite;
    public Sprite cannotAffordSprite;
    public Sprite prerequisiteRequiredSprite;
    // Label is probably going to be deprecated
    public TextMeshProUGUI label;

    public Image textBox;
    public TextMeshProUGUI description;

    private TowerDefinition tower;
    private int index;
    private PerkDefinition perk;

    public void Setup(TowerDefinition t, int i)
    {
        tower = t;
        index = i;
        perk = t.perks[i];
        name = perk.name;
        button.onClick.AddListener(() => PerkManager.Instance.TryBuy(tower, index));
    }

    void Update()
    {
        if (perk == null) return;
        PerkManager pm = PerkManager.Instance;

        button.image.sprite = perk.image;
        textBox.sprite = perk.textBoxImage;
        label.text = $"{perk.displayName} ({perk.cost})";

        PerkState state = pm.GetState(tower, index);
        button.interactable = state == PerkState.Available;

        // Only show up to two steps past what you own, and nothing past a perk the AI took
        bool hide = pm.GetState(tower, index - 2) != PerkState.Owned
                 || pm.GetState(tower, index - 1) == PerkState.Revoked;

        description.text = GetTextboxText(hide ? PerkState.Hidden : state);
        background.enabled = !hide;
        overlay.enabled = !hide;
        if (hide) return;

        switch (state)
        {
            case PerkState.Owned:
                overlay.sprite = ownedSprite;
                break;
            case PerkState.Revoked:
                overlay.sprite = lockedSprite;
                break;
            case PerkState.CantAfford:
                overlay.sprite = cannotAffordSprite;
                break;
            case PerkState.NeedsPrevious:
                overlay.sprite = prerequisiteRequiredSprite;
                break;
            default: // Available
                overlay.enabled = false;
                break;
        }
    }

    string GetTextboxText(PerkState state)
    {
        GameStrings s = GameStrings.Current;
        string cost = $"{perk.cost} {s.skillPointsName}";
        if (state == PerkState.CantAfford) cost += "\n<b>CANNOT AFFORD</b>";
        string tierLine = perk.unlocksTier > 0
            ? "\n<b>" + string.Format(s.unlocksTierFormat, tower.displayName, tower.GetTier(perk.unlocksTier).name) + "</b>"
            : "";

        switch (state)
        {
            case PerkState.Hidden:
                return null;
            case PerkState.NeedsPrevious:
                return "<b>???</b>\nNeeds previous perk";
            case PerkState.Revoked:
                return $"<b>{perk.displayName}</b>\n{s.perkTakenLabel}";
            default:
                return $"<b>{perk.displayName}</b>\n{cost}{tierLine}\n\n{perk.description}";
        }
    }
}
