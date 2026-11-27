using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CompositionChip : MonoBehaviour
{
    public Image background;
    public Image swatch;
    public Image icon;
    public TextMeshProUGUI label;
    public TextMeshProUGUI tierText;
    public TooltipTrigger tooltip;

    public void Bind(EnemyDefinition def, int count)
    {
        Color fc = def.faction != null ? def.faction.color : Color.white;
        if (background != null) background.color = new Color(fc.r, fc.g, fc.b, 0.18f);
        if (swatch != null) swatch.color = fc;
        if (icon != null)
        {
            icon.sprite = def.icon != null ? def.icon : (def.faction != null ? def.faction.icon : null);
            icon.enabled = icon.sprite != null;
        }
        label.text = $"{def.displayName} ×{count}";
        if (tierText != null) tierText.text = def.tierLabel;
        if (tooltip != null)
        {
            string company = def.faction != null ? def.faction.displayName : "";
            tooltip.Provider = () => (def.displayName,
                $"{company}  ·  tier {def.tier}\nHealth {def.maxHealth}  ·  Speed {def.speed}\nBreach damage {def.damageToBase}  ·  Reward {def.reward}" +
                (string.IsNullOrEmpty(def.description) ? "" : $"\n\n{def.description}"), fc);
        }
    }
}
