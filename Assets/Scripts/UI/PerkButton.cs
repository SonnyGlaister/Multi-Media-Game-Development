using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

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

    private bool hidePerkButton;
    private FactionDefinition faction;
    private int tier;
    private PerkDefinition perk;

    public Image textBox;
    public TextMeshProUGUI description;

    public void Setup(FactionDefinition f, int t)
    {
        faction = f;
        tier = t;
        perk = f.perks[t];
        name = perk.name;
        button.onClick.AddListener(() => PerkManager.Instance.TryBuy(faction, tier));
    }

    void Update()
    {
        if (perk == null) return;

        button.image.sprite = perk.image;
        PerkState state = PerkManager.Instance.GetState(faction, tier);
        button.interactable = state == PerkState.Available;

        textBox.sprite = perk.textBoxImage;

        // We check if the grandparent's state is bought because that is more specific than checking if the parent needs previous
        PerkState grandParentState = PerkManager.Instance.GetState(faction, tier - 2);
        PerkState parentState = PerkManager.Instance.GetState(faction, tier - 1);

        hidePerkButton = (tier >= 2 && grandParentState != PerkState.Owned) || (tier >= 1 && parentState == PerkState.LockedOut);
        description.text = GetTextboxText(hidePerkButton ? PerkState.Hidden : state );

        if (hidePerkButton)
        {
            background.enabled = false;
            overlay.enabled = false;

            return;
        } else
        {
            background.enabled = true;
            overlay.enabled = true;
        }

        switch (state)
        {
            case PerkState.Available:
                overlay.enabled = false;
                break;
            case PerkState.Owned:
                overlay.enabled = true;
                overlay.sprite = ownedSprite;
                break;
            case PerkState.LockedOut:
            case PerkState.Revoked:
                overlay.enabled = true;
                overlay.sprite = lockedSprite;
                break;
            case PerkState.CantAfford:
                overlay.enabled = true;
                overlay.sprite = cannotAffordSprite;
                break;
            case PerkState.NeedsPrevious:
                overlay.enabled = true;
                overlay.sprite = prerequisiteRequiredSprite;
                break;
            default: // CantAfford, NeedsPrevious
                overlay.enabled = false;
                break;
        }
    }

    string GetTextboxText (PerkState state)
    {
        string costText = state == PerkState.CantAfford
        ? $"{perk.cost} gold\n<b>CANNOT AFFORD</b>"
        :$"{perk.cost} gold";
        switch (state)
        {
            case PerkState.Hidden:
                return null;
            case PerkState.NeedsPrevious:
                return $"<b>???</b>\nNeeds Previous Perk";
            case PerkState.LockedOut:
                return $"<b>Revoked</b>\nThis path has been blocked";
            default:
                return $"<b>{perk.displayName}</b>\n{costText}\n\n{perk.description}";
        }
    }
}