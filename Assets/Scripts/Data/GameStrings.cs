using UnityEngine;

// All player-facing text
[CreateAssetMenu(menuName = "Game/Game Strings")]
public class GameStrings : ScriptableObject
{
    private static GameStrings current;
    public static GameStrings Current
    {
        get
        {
            if (current == null) current = Resources.Load<GameStrings>("GameStrings");
            if (current == null)
            {
                current = CreateInstance<GameStrings>();
                Debug.LogWarning("GameStrings: no Assets/Resources/GameStrings.asset found, using defaults.");
            }
            return current;
        }
    }

    [Header("Resources")]
    public string skillPointsName = "Skill Points";
    public string skillTreeTitle = "Skill Tree";
    public string baseName = "Human HQ";
    public string baseHitFormat = "{0} breached {1} (-{2})";

    [Header("Waves")]
    public string waveFormat = "Wave {0} / {1}";
    public string waveBannerFormat = "WAVE {0}";
    public string finalWave = "FINAL WAVE";
    public string startPrompt = "Press Start, then build defences before the first wave arrives.";
    public string startButton = "Start";
    public string waveClearedFormat = "Wave {0} repelled";

    [Header("AI pressure")]
    public string pressureLabel = "AI Pressure";
    public string atRiskFormat = "At risk: {0}";
    public string nothingAtRisk = "No skills at risk";
    public string perkTakenFormat = "The AI acquired your {0}";
    public string perkTakenLabel = "Acquired by the AI";
    [Tooltip("{0} = HQ name, {1} = where the bar drops back to")]
    public string pressureTooltip = "Every AI model that reaches {0} adds pressure. It slowly fades over time.\nWhen the bar fills, the AI takes your most recent skill and the bar drops back to {1:P0}.";

    [Header("Towers")]
    public string buildTowersButton = "Build  [B]";
    public string hideTowersButton = "Hide  [B]";
    public string nextCostFormat = "next {0}";
    public string towerHint = "Drag onto the map, or click then click the map.\nRight-click / Esc cancels. Hold Shift to place several.";
    [Tooltip("{0} = tower faction, {1} = tier number")]
    public string towerHoverHint = "{0}  ·  Tier {1}\n<size=80%>Click for details</size>";
    public string tierFormat = "Tier {0}";
    public string tierUpFormat = "{0} towers upgraded to {1}";
    public string tierDownFormat = "The AI downgraded your {0} towers to {1}";
    public string unlocksTierFormat = "Upgrades {0} towers to {1}";
    public string sellFormat = "Sell  +{0}";
    public string sellConfirmFormat = "Click again to sell  +{0}";

    [Header("Placement errors")]
    public string errOnPath = "Can't build on the route";
    public string errNearTower = "Too close to another tower";
    public string errNearBase = "Too close to HQ";
    public string errNeedSkillPoints = "Need {0} more";

    [Header("End screen")]
    public string victoryTitle = "HUMANITY HOLDS";
    public string defeatTitle = "HQ OVERRUN";
    public string wavesSurvivedFormat = "{0} / {1} waves survived";
}
