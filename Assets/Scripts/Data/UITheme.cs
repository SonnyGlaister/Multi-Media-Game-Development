using TMPro;
using UnityEngine;

// Shared UI colours and fonts
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

    [Header("Panels & text")]
    public Color panel = new Color(0.07f, 0.08f, 0.10f, 0.88f);
    public Color panelRaised = new Color(0.12f, 0.13f, 0.16f, 0.95f);
    public Color panelBorder = new Color(1f, 1f, 1f, 0.08f);
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

    [Header("Fonts (optional)")]
    public TMP_FontAsset displayFont;
    public TMP_FontAsset bodyFont;
    public TMP_FontAsset monoFont;

    public Color MessageColor(MessageType type) => type switch
    {
        MessageType.Success => success,
        MessageType.Warning => warning,
        MessageType.Danger => danger,
        _ => info,
    };

    public static string Hex(Color c) => ColorUtility.ToHtmlStringRGB(c);
}