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

    [Header("AI pressure")]
    [Tooltip("When pressure reaches this, the AI takes your most recent skill.")]
    public float pressureLimit = 5f;
    [Tooltip("After the AI takes a skill, pressure drops back to this fraction of the limit.")]
    [Range(0f, 1f)] public float pressureAfterRevoke = 0.75f;
    [Tooltip("Pressure that fades per second while the level is running.")]
    public float pressureDecayPerSecond = 0.05f;

    [Header("References")]
    public EnemySpawner spawner;

    // Events
    public event Action<int> SkillPointsChanged;
    public event Action<int> PurchaseFailed;
    public event Action<int, int> BaseHealthChanged;
    public event Action<EnemyDefinition, int> BaseHit;
    public event Action<float> PressureAdded;
    public event Action PressureMaxed;
    public event Action<int> SetupStarted;
    public event Action<int> WaveStarted;
    public event Action<bool> GameEnded;
    public event Action<string, MessageType> MessageRequested;

    public FactionDefinition[] Factions { get; private set; }
    public GameStats Stats { get; } = new GameStats();

    private readonly Dictionary<FactionDefinition, int> totalHits = new();

    private int skillPoints;
    public int SkillPoints
    {
        get => skillPoints;
        private set { skillPoints = value; SkillPointsChanged?.Invoke(skillPoints); }
    }

    public float Pressure { get; private set; }
    public float Pressure01 => pressureLimit > 0f ? Mathf.Clamp01(Pressure / pressureLimit) : 0f;

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
        Time.timeScale = 3f;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Update()
    {
        if (IsGameOver || !HasGameStarted) return;

        if (Pressure > 0f)
            Pressure = Mathf.Max(0f, Pressure - pressureDecayPerSecond * Time.deltaTime);

        if (HasWaveStarted) CheckWaveEnd();
    }

    // AI companies (end screen)
    public int GetTotalHits(FactionDefinition f) => f != null && totalHits.TryGetValue(f, out int v) ? v : 0;

    // AI pressure
    public void AddPressure(float amount)
    {
        if (amount <= 0f || IsGameOver) return;
        Pressure += amount;
        PressureAdded?.Invoke(amount);

        if (Pressure >= pressureLimit)
        {
            Pressure = pressureLimit * pressureAfterRevoke;
            PressureMaxed?.Invoke();
        }
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

    public void Refund(int amount)
    {
        if (amount > 0) SkillPoints += amount;
    }

    // HQ
    public void EnemyReachedBase(EnemyDefinition definition)
    {
        if (IsGameOver) return;
        BaseHealth = Mathf.Max(BaseHealth - definition.damageToBase, 0);
        BaseHealthChanged?.Invoke(BaseHealth, MaxBaseHealth);
        BaseHit?.Invoke(definition, definition.damageToBase);

        FactionDefinition f = definition.faction;
        if (f != null) totalHits[f] = GetTotalHits(f) + 1;
        AddPressure(definition.pressurePerHit);

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

    public void StartWave()
    {
        if (HasWaveStarted) return;
        HasWaveStarted = true;
        WaveStarted?.Invoke(currentWave);
    }

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
