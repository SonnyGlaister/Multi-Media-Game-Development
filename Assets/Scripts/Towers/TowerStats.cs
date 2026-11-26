using System.Text;
using UnityEngine;

// Every number a tower can have
[System.Serializable]
public class TowerStats
{
    [Header("All towers")]
    public float range;
    [Tooltip("Seconds between attacks.")]
    public float cooldown;
    public int damage;
    [Tooltip("Splash radius. 0 = no splash.")]
    public float aoeRadius;

    [Header("Physical (projectiles)")]
    [Tooltip("Different enemies shot at once (multishot).")]
    public int projectiles;
    public int piercing;
    public float projectileSpeed;

    public TowerStats Copy() => (TowerStats)MemberwiseClone();

    public void Add(TowerStats bonus)
    {
        range += bonus.range;
        cooldown += bonus.cooldown;
        damage += bonus.damage;
        aoeRadius += bonus.aoeRadius;
        projectiles += bonus.projectiles;
        piercing += bonus.piercing;
        projectileSpeed += bonus.projectileSpeed;
    }

    public static string Describe(TowerStats baseStats, TowerStats stats)
    {
        string bonusHex = UITheme.Hex(UITheme.Current.bonus);
        StringBuilder sb = new();

        void Line(string label, float baseValue, float value, string format)
        {
            if (baseValue == 0f && value == 0f) return;
            string v = Mathf.Approximately(baseValue, value)
                ? value.ToString(format)
                : $"{baseValue.ToString(format)} → <color=#{bonusHex}>{value.ToString(format)}</color>";
            sb.Append($"{label}: {v}\n");
        }

        Line("Damage", baseStats.damage, stats.damage, "0");
        Line("Attacks/sec", PerSecond(baseStats.cooldown), PerSecond(stats.cooldown), "0.##");
        Line("Range", baseStats.range, stats.range, "0.#");
        if (baseStats.projectiles > 1 || stats.projectiles > 1) Line("Shots", baseStats.projectiles, stats.projectiles, "0");
        Line("Pierce", baseStats.piercing, stats.piercing, "0");
        Line("Splash", baseStats.aoeRadius, stats.aoeRadius, "0.#");
        return sb.ToString().TrimEnd('\n');
    }

    private static float PerSecond(float cooldown) => cooldown > 0f ? 1f / cooldown : 0f;
}
