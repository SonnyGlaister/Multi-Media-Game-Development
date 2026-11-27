using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(Graphic))]
public class ThemedGraphic : MonoBehaviour
{
    public enum Role { None, Panel, PanelRaised, PanelBorder, TextPrimary, TextMuted, SkillPoints, Health, Danger, Warning, Success, Info, Disabled }
    public enum FontRole { Keep, Display, Body, Mono }

    public Role role = Role.Panel;
    [Range(0f, 1f)] public float alpha = 1f;
    public FontRole font = FontRole.Keep;

    private void OnEnable() => Apply();
    private void OnValidate() => Apply();

    public void Apply()
    {
        Graphic g = GetComponent<Graphic>();
        if (g == null) return;
        UITheme t = UITheme.Current;

        if (role != Role.None)
        {
            Color c = role switch
            {
                Role.Panel => t.panel,
                Role.PanelRaised => t.panelRaised,
                Role.PanelBorder => t.panelBorder,
                Role.TextPrimary => t.textPrimary,
                Role.TextMuted => t.textMuted,
                Role.SkillPoints => t.skillPoints,
                Role.Health => t.health,
                Role.Danger => t.danger,
                Role.Warning => t.warning,
                Role.Success => t.success,
                Role.Info => t.info,
                Role.Disabled => t.disabled,
                _ => g.color,
            };
            c.a *= alpha;
            g.color = c;
        }

        if (g is TMP_Text text && font != FontRole.Keep)
        {
            TMP_FontAsset f = font switch
            {
                FontRole.Display => t.displayFont,
                FontRole.Body => t.bodyFont,
                FontRole.Mono => t.monoFont,
                _ => null,
            };
            if (f != null) text.font = f;
        }
    }
}
