using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PerkButton : MonoBehaviour
{
    public Button button;
    public Image background;
    // Label is probably going to be deprecated
    public TextMeshProUGUI label;

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
        label.text = $"{perk.displayName} ({perk.cost})";
        description.text = $"<b>{perk.displayName}</b>\n {perk.cost} gold\n\n {perk.description}";
        textBox.sprite = perk.textBoxImage;

        switch (state)
        {
            case PerkState.Available:
                background.color = faction.color;
                break;
            case PerkState.Owned:
                background.color = Color.green;
                label.text = "Bought";
                break;
            case PerkState.LockedOut:
            case PerkState.Revoked:
                background.color = Color.black;
                label.text = "Disabled";
                break;
            default: // CantAfford, NeedsPrevious
                background.color = Color.gray;
                break;
        }
    }
}