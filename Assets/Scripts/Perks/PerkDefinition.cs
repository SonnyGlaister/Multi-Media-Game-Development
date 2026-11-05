using UnityEngine;

[CreateAssetMenu(menuName = "Game/Perk")]
public class PerkDefinition : ScriptableObject
{
    public string displayName = "Perk";
    public int cost = 10;

    public float cooldownMultiplier = 1f;
    public float rangeMultiplier = 1f;
    public int damageBonus = 0;
}