using UnityEngine;

[CreateAssetMenu(menuName = "Game/Perk")]
public class PerkDefinition : ScriptableObject
{
    // Bonuses should be used to add, Multipliers should be used to multiply
    public string displayName = "Perk";
    public int cost = 10;
    public float cooldownMultiplier = 1f;
    public float rangeMultiplier = 1f;
    public int piercingBonus = 0;
    public int damageBonus = 0;
    public float aoeRangeBonus = 0;
// AOE damage is represented by a multiple of the base damage but is added to the original value i.e first value can be 0.2 and then next could be 0.3 which would make 0.5 maybe that's dumb
    public float aoeDamageMultiplier = 0f;
    public string description = "";
    public Sprite image;
    public Sprite textBoxImage;
}