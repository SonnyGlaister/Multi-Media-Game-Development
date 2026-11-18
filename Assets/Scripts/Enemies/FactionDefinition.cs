using UnityEngine;

// An attacking AI company
[CreateAssetMenu(menuName = "Game/Faction")]
public class FactionDefinition : ScriptableObject
{
    public string displayName;
    public Color color;
    public int lockoutThreshold;
    public PerkDefinition[] perks;
    public Sprite icon;
}