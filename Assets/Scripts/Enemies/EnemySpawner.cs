using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public EnemyController enemyPrefab;
    public EnemyDefinition[] enemyTypes;

    public float setupTime = 3; // Time between waves
    public int[][] enemySpawnNumbers =
    {
        new int[] {10,-1, -1},
        new int[] {8,6, -1},
        new int[] {8,10, -1},
        new int[] {2, 15, -1},
        new int[] {20, 20, 1}
    }; // Set amounts of enemy types for all waves. If -1, cannot be randomly spawned in wave.

    public float enemySpawnCooldown; // Time between enemy spawns in a wave

    private int enemySpawnNumber = 0;
    public float SetupTimeRemaining { get; private set; }
    public bool FinishedSpawning => spawnedEnemyTotal >= enemySpawnNumber;

    // UI info
    public int WaveCount => enemySpawnNumbers.Length;
    public int TotalThisWave => enemySpawnNumber;
    public int SpawnedThisWave => spawnedEnemyTotal;
    public float SetupDuration => setupTime;

    private int spawnedEnemyTotal;
    private float timer;
    private int waveNum = 0;

    private float RANDOMDIVISOR = 2.2F;
    private int[][] randomisedEnemySpawnNumbers;
    private int[] waveSnapshot; // counts at the start of the wave (for the preview)
    private GameObject baseTarget;

    private void Awake()
    {
        Random.InitState(System.DateTime.Now.Millisecond);

        randomisedEnemySpawnNumbers = cloneJaggedArray(enemySpawnNumbers);
        waveNum = 0;
        SetupTimeRemaining = setupTime;
        getTotalEnemies();
        timer = enemySpawnCooldown;
        TakeSnapshot();
    }

    private void Start()
    {
        baseTarget = GameObject.FindWithTag("Base");
    }

    private void Update()
    {
        if (!GameController.Instance.HasGameStarted || GameController.Instance.IsGameOver) return;

        if (InSetup())
        {
            SetupTimeRemaining -= Time.deltaTime;
            return;
        }

        if (!GameController.Instance.HasWaveStarted)
        {
            GameController.Instance.StartWave();
        }

        if (FinishedSpawning) return;

        timer += Time.deltaTime;
        if (timer >= enemySpawnCooldown)
        {
            int select = 0;
            for (int i = 0; i < randomisedEnemySpawnNumbers[waveNum].Length; i++)
            {
                if (randomisedEnemySpawnNumbers[waveNum][i] != 0)
                {
                    select = i;
                    break;
                }
            }

            EnemyController enemy = Instantiate(enemyPrefab, transform.position, Quaternion.identity);
            enemy.Init(enemyTypes[select]);
            enemy.SetTargetDestination(baseTarget.transform.position);

            spawnedEnemyTotal++;
            randomisedEnemySpawnNumbers[waveNum][select] -= 1;
            timer = 0;
        }
    }

    private bool InSetup()
    {
        if (SetupTimeRemaining > 0) return true;
        if (!GameController.Instance.HasWaveStarted) GameController.Instance.StartWave();
        return false;
    }

    // Called before every round (besides 0).
    public void ResetSpawner()
    {
        waveNum = GameController.Instance.currentWave;
        spawnedEnemyTotal = 0;
        SetupTimeRemaining = setupTime;
        timer = enemySpawnCooldown;
        standardRandomisation();
        TakeSnapshot();
    }

    // Ends the setup countdown now
    public float SkipSetup()
    {
        float skipped = Mathf.Max(0f, SetupTimeRemaining);
        SetupTimeRemaining = 0f;
        return skipped;
    }

    // Fills 'into' with (enemy type, count) for the current/upcoming wave.
    public void GetComposition(List<(EnemyDefinition def, int count)> into)
    {
        into.Clear();
        if (waveSnapshot == null) return;
        for (int i = 0; i < waveSnapshot.Length && i < enemyTypes.Length; i++)
        {
            if (waveSnapshot[i] > 0 && enemyTypes[i] != null) into.Add((enemyTypes[i], waveSnapshot[i]));
        }
    }

    private void TakeSnapshot()
    {
        if (randomisedEnemySpawnNumbers == null || waveNum >= randomisedEnemySpawnNumbers.Length) return;
        waveSnapshot = (int[])randomisedEnemySpawnNumbers[waveNum].Clone();
    }

    // Logarithmic wave randomisation
    private void standardRandomisation()
    {
        getTotalEnemies();
        int l = randomisedEnemySpawnNumbers[waveNum].Length;

        float randomChance = Mathf.Log10(waveNum);
        List<int> types = new List<int>();

        randomChance = randomChance / RANDOMDIVISOR;
        if (randomChance > 0.7) randomChance = 0.7F;

        for (int j = 0; j < l; j++)
        {
            if (randomisedEnemySpawnNumbers[waveNum][j] != -1 && randomisedEnemySpawnNumbers[waveNum][j] != 1)
            {
                types.Add(j);
            }
        }

        int totalRandomised = (int)(randomChance * enemySpawnNumber);
        for (int i = 0; i < totalRandomised; i++)
        {
            int r1 = types[Random.Range(0, types.Count)];
            randomisedEnemySpawnNumbers[waveNum][r1] -= 1;

            int r2 = types[Random.Range(0, types.Count)];
            randomisedEnemySpawnNumbers[waveNum][r2] += 1;
        }
    }

    // Total enemies to spawn during the upcoming wave
    private void getTotalEnemies()
    {
        int sum = 0;
        foreach (int num in randomisedEnemySpawnNumbers[waveNum])
        {
            if (num == -1) continue;
            sum += num;
        }
        enemySpawnNumber = sum;
    }

    // Used to clone original enemy spawn numbers array before randomisation.
    public int[][] cloneJaggedArray(int[][] original)
    {
        if (original == null) return null;
        int[][] clone = new int[original.Length][];
        for (int i = 0; i < original.Length; i++)
        {
            clone[i] = original[i] != null ? (int[])original[i].Clone() : null;
        }
        return clone;
    }
}