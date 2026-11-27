using UnityEngine;

// One tower faction (Physical, Magic, Mechanic, Imaginary)
[CreateAssetMenu(menuName = "Game/Tower")]
public class TowerDefinition : ScriptableObject
{
    [Header("Faction")]
    public string displayName = "Physical";
    public Color color = Color.white;
    public Sprite icon;
    [TextArea] public string description = "";

    [Header("Prefab")]
    [Tooltip("TowerController + one attack component + an empty Model child + RangeRing.")]
    public TowerController prefab;

    [Header("Tiers (tier 1 first)")]
    public TowerTier[] tiers;

    [Header("Skill tree branch (top to bottom)")]
    public PerkDefinition[] perks;

    [Header("Economy")]
    public int startCost = 5;
    [Tooltip("How many of this tower cost Start Cost before the price grows.")]
    public int flatPriceCount = 2;
    [Tooltip("Each tower after that costs this many times more.")]
    public float costGrowth = 2f;
    [Range(0f, 1f)] public float sellRefund = 0.7f;

    public int MaxTier => tiers.Length;
    public TowerTier GetTier(int tier) => tiers[Mathf.Clamp(tier, 1, tiers.Length) - 1];

    public int CostFor(int alreadyPlaced)
    {
        if (alreadyPlaced < flatPriceCount) return startCost;
        return Mathf.RoundToInt(startCost * Mathf.Pow(costGrowth, alreadyPlaced - flatPriceCount + 1));
    }
}

[System.Serializable]
public class TowerTier
{
    public string name = "Crossbow";
    public Sprite icon;
    [TextArea] public string description = "";
    [Tooltip("Visual only (no colliders, no scripts). Spawned under the tower's Model child.")]
    public GameObject model;
    [Tooltip("Physical: the projectile prefab. Imaginary: the summon prefab. Magic/Mechanic: leave empty.")]
    public GameObject spawnPrefab;
    public TowerStats stats;
}
