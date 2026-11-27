using UnityEngine;

[CreateAssetMenu(menuName = "Game/Perk")]
public class PerkDefinition : ScriptableObject
{
    public string displayName = "Perk";
    public int cost = 10;
    [TextArea] public string description = "";
    public Sprite image;
    public Sprite textBoxImage;

    [Header("Tier upgrade")]
    [Tooltip("0 = normal perk. 2 or 3 = buying this upgrades this branch's towers to that tier.")]
    public int unlocksTier = 0;

    [Header("Stat changes (only this branch's towers)")]
    [Tooltip("Multiplies the time between attacks. 0.85 = 15% faster.")]
    public float cooldownMultiplier = 1f;
    [Tooltip("Multiplies range. 1.2 = 20% further.")]
    public float rangeMultiplier = 1f;
    [Tooltip("Added on top of the tier's stats. Leave a field at 0 to not change it.")]
    public TowerStats bonus = new TowerStats();
}
