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
    public string baseName = "Human HQ";
    public string baseHitFormat = "{0} breached {1} (-{2})";

    [Header("Waves")]
    public string waveFormat = "Wave {0} / {1}";
    public string waveBannerFormat = "WAVE {0}";
    public string finalWave = "FINAL WAVE";
    public string nextWaveTitle = "Incoming";
    public string startPrompt = "Press Start, then build defences before the first wave arrives.";
    public string startButton = "Start";
    public string callNowButton = "Call now";
    public string earlyBonusFormat = "+{0} {1} for calling early";
    public string waveClearedFormat = "Wave {0} repelled";

    [Header("Factions (AI companies)")]
    public string pressureLabel = "Influence";
    public string lockedLabel = "CAPTURED";
    public string nextHitRevokesFormat = "Next breach: lose {0}";
    public string revokedToastFormat = "{0} acquired your {1}";
    public string lockedToastFormat = "{0} has captured its research branch";
    public string pressureTooltipFormat = "Every {0} model that reaches {1} adds influence.\nWhen the bar fills, {0} acquires your highest {0} upgrade and you can't research it again.";

    [Header("End screen")]
    public string victoryTitle = "HUMANITY HOLDS";
    public string defeatTitle = "HQ OVERRUN";
    public string wavesSurvivedFormat = "{0} / {1} waves survived";
}