using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Next wave card tile
public class FactionPill : MonoBehaviour
{
    public Image background;
    [Tooltip("The company logo (FactionDefinition.icon). Keep a circle sprite on it as the fallback.")]
    public Image logo;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI countText;

    private Sprite placeholder;

    private void Awake() => placeholder = logo.sprite;

    public void Bind(FactionDefinition faction, int count)
    {
        Color c = faction != null ? faction.color : Color.white;
        if (background != null) background.color = new Color(c.r, c.g, c.b, 0.18f);

        bool hasLogo = faction != null && faction.icon != null;
        logo.sprite = hasLogo ? faction.icon : placeholder;
        logo.color = hasLogo ? Color.white : c; // real logos keep their own colours

        nameText.text = faction != null ? faction.displayName : "Unknown";
        countText.text = $"×{count}";
    }
}