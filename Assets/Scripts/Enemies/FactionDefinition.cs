using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Faction")]
public class FactionDefinition : ScriptableObject
{
    public string displayName;
    public Color color;
    public int lockoutThreshold;
    public PerkDefinition[] perks;
}
