using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PerkButton : MonoBehaviour
{
    public Button button;
    public Image background;
    public TextMeshProUGUI label;

    private FactionDefinition faction;
    private int tier;
    private PerkDefinition perk;

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

        PerkState state = PerkManager.Instance.GetState(faction, tier);
        button.interactable = state == PerkState.Available;
        label.text = $"{perk.displayName} ({perk.cost})";

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