using UnityEngine;

// Shared UI colours
[CreateAssetMenu(menuName = "Game/UI Theme")]
public class UITheme : ScriptableObject
{
    private static UITheme current;
    public static UITheme Current
    {
        get
        {
            if (current == null) current = Resources.Load<UITheme>("UITheme");
            if (current == null)
            {
                current = CreateInstance<UITheme>();
                Debug.LogWarning("UITheme: no Assets/Resources/UITheme.asset found, using defaults.");
            }
            return current;
        }
    }

    [Header("Text")]
    public Color textPrimary = new Color(0.93f, 0.95f, 0.97f, 1f);
    public Color textMuted = new Color(0.60f, 0.65f, 0.72f, 1f);

    [Header("Semantic")]
    public Color skillPoints = new Color(0.96f, 0.77f, 0.32f, 1f);
    public Color health = new Color(0.36f, 0.86f, 0.62f, 1f);
    public Color danger = new Color(0.95f, 0.33f, 0.31f, 1f);
    public Color warning = new Color(0.98f, 0.70f, 0.25f, 1f);
    public Color success = new Color(0.38f, 0.86f, 0.46f, 1f);
    public Color info = new Color(0.36f, 0.72f, 0.98f, 1f);
    public Color disabled = new Color(0.24f, 0.26f, 0.30f, 1f);

    [Header("Towers")]
    public Color bonus = new Color(0.45f, 0.95f, 0.55f, 1f);
    public Color validPlacement = new Color(0.36f, 0.90f, 1f, 0.9f);
    public Color invalidPlacement = new Color(1f, 0.30f, 0.30f, 0.9f);
    public Color selectedRange = new Color(0.36f, 0.90f, 1f, 0.35f);
    public Color hoverRange = new Color(1f, 1f, 1f, 0.2f);
    public Color neighbourRange = new Color(1f, 1f, 1f, 0.08f);
    public Color noBuild = new Color(1f, 0.25f, 0.25f, 0.35f);

    public Color MessageColor(MessageType type) => type switch
    {
        MessageType.Success => success,
        MessageType.Warning => warning,
        MessageType.Danger => danger,
        _ => info,
    };

    public static string Hex(Color c) => ColorUtility.ToHtmlStringRGB(c);
}