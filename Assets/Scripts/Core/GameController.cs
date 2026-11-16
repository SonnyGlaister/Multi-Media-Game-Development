using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }

    [Header("Rules")]
    public int startingSkillPoints = 5;
    public int startingBaseHealth = 3;
    [Tooltip("Waves to play. 0 = every wave defined in the spawner (capped at that number either way).")]
    public int totalWaves = 2;
    [Tooltip("Skill points per second of setup time skipped with 'Call now'. 0 = no bonus.")]
    public float earlyCallSkillPointsPerSecond = 0f;

    [Header("References")]
    public EnemySpawner spawner;

    // Events
    public event Action<int> SkillPointsChanged; // new amount
    public event Action<int> PurchaseFailed; // unpaid cost
    public event Action<int, int> BaseHealthChanged; // current, max
    public event Action<EnemyDefinition, int> BaseHit; // enemy, damage
    public event Action<FactionDefinition, int> PressureChanged; // faction, new pressure
    public event Action<int> SetupStarted; // wave index
    public event Action<int> WaveStarted; // wave index
    public event Action<bool> GameEnded; // won?
    public event Action<string, MessageType> MessageRequested;

    public FactionDefinition[] Factions { get; private set; }
    public GameStats Stats { get; } = new GameStats();

    private readonly Dictionary<FactionDefinition, int> totalHits = new();
    private readonly Dictionary<FactionDefinition, int> pressure = new();

    private int skillPoints;
    public int SkillPoints
    {
        get => skillPoints;
        private set { skillPoints = value; SkillPointsChanged?.Invoke(skillPoints); }
    }

    public int BaseHealth { get; private set; }
    public int MaxBaseHealth => startingBaseHealth;
    public bool IsGameOver { get; private set; }
    public bool HasGameStarted { get; private set; }
    public bool HasWaveStarted { get; private set; }
    public int currentWave { get; private set; }

    public int TotalWaves
    {
        get
        {
            if (spawner == null) return Mathf.Max(1, totalWaves);
            return totalWaves > 0 ? Mathf.Min(totalWaves, spawner.WaveCount) : spawner.WaveCount;
        }
    }
    public bool IsFinalWave => currentWave >= TotalWaves - 1;

    // Lifecycle
    private void Awake()
    {
        Instance = this;
        if (spawner == null) spawner = FindFirstObjectByType<EnemySpawner>();
        Factions = spawner.enemyTypes.Where(t => t != null && t.faction != null).Select(t => t.faction).Distinct().ToArray();

        skillPoints = startingSkillPoints;
        BaseHealth = startingBaseHealth;
        currentWave = 0;
        Time.timeScale = 1f;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Update()
    {
        if (IsGameOver || !HasWaveStarted) return;
        CheckWaveEnd();
    }

    // Factions
    public int GetTotalHits(FactionDefinition f) => f != null && totalHits.TryGetValue(f, out int v) ? v : 0;
    public int GetPressure(FactionDefinition f) => f != null && pressure.TryGetValue(f, out int v) ? v : 0;
    public bool IsLockedOut(FactionDefinition f) => f != null && GetPressure(f) >= f.lockoutThreshold;

    public void ReducePressure(FactionDefinition f, int amount)
    {
        pressure[f] = Mathf.Max(0, GetPressure(f) - amount);
        PressureChanged?.Invoke(f, pressure[f]);
    }

    // Economy
    public void EnemyKilled(EnemyController enemy)
    {
        if (IsGameOver || enemy == null || enemy.Definition == null) return;
        int reward = enemy.Definition.reward;
        Stats.enemiesKilled++;
        Stats.skillPointsEarned += reward;
        SkillPoints += reward;
    }

    public bool CanAfford(int cost) => SkillPoints >= cost;

    // Spends skill points if affordable
    public bool Purchase(int cost)
    {
        if (IsGameOver) return false;
        if (!CanAfford(cost))
        {
            PurchaseFailed?.Invoke(cost);
            return false;
        }
        Stats.skillPointsSpent += cost;
        SkillPoints -= cost;
        return true;
    }

    // HQ
    public void EnemyReachedBase(EnemyDefinition definition)
    {
        if (IsGameOver) return;
        BaseHealth = Mathf.Max(BaseHealth - definition.damageToBase, 0);
        BaseHealthChanged?.Invoke(BaseHealth, MaxBaseHealth);
        BaseHit?.Invoke(definition, definition.damageToBase);

        FactionDefinition f = definition.faction;
        if (f != null)
        {
            totalHits[f] = GetTotalHits(f) + 1;
            pressure[f] = GetPressure(f) + definition.pressurePerHit;
            PressureChanged?.Invoke(f, pressure[f]);
        }

        if (BaseHealth <= 0) EndGame(false);
    }

    // Messages
    public void ShowMessage(string message, MessageType type = MessageType.Info) =>
        MessageRequested?.Invoke(message, type);

    // Waves
    public void StartGame()
    {
        if (HasGameStarted) return;
        HasGameStarted = true;
        SetupStarted?.Invoke(currentWave);
    }

    // Called by the spawner when the setup countdown ends
    public void StartWave()
    {
        if (HasWaveStarted) return;
        HasWaveStarted = true;
        WaveStarted?.Invoke(currentWave);
    }

    // Skips the remaining setup time, with an optional bonus
    public void CallWaveEarly()
    {
        if (!HasGameStarted || HasWaveStarted || IsGameOver || spawner == null) return;
        float skipped = spawner.SkipSetup();
        int bonus = Mathf.FloorToInt(skipped * earlyCallSkillPointsPerSecond);
        if (bonus > 0)
        {
            Stats.skillPointsEarned += bonus;
            SkillPoints += bonus;
            ShowMessage(string.Format(GameStrings.Current.earlyBonusFormat, bonus, GameStrings.Current.skillPointsName), MessageType.Success);
        }
    }

    public int EarlyCallBonusPreview =>
        spawner == null ? 0 : Mathf.FloorToInt(spawner.SetupTimeRemaining * earlyCallSkillPointsPerSecond);

    private void CheckWaveEnd()
    {
        if (spawner == null || !spawner.FinishedSpawning) return;
        if (EnemyController.Alive.Count > 0) return;

        int cleared = currentWave;
        Stats.wavesCleared++;
        currentWave++;

        if (currentWave >= TotalWaves)
        {
            EndGame(true);
        }
        else
        {
            ShowMessage(string.Format(GameStrings.Current.waveClearedFormat, cleared + 1), MessageType.Success);
            SetupWave();
        }
    }

    private void SetupWave()
    {
        HasWaveStarted = false;
        spawner.ResetSpawner();
        SetupStarted?.Invoke(currentWave);
    }

    private void EndGame(bool won)
    {
        if (IsGameOver) return;
        IsGameOver = true;
        Time.timeScale = 0f;
        GameEnded?.Invoke(won);
    }

    // Scenes
    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }

    public void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return;
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }
}