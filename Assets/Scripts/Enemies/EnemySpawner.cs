using System.Collections;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    public EnemyController enemyPrefab;
    public EnemySpawnBlockDefinition[] spawnBlocks;
    public int waveNum { get; private set; }

    private GameObject baseTarget;
    private bool spawning=false;

    public bool FinishedSpawning => spawnedBlockTotal >= blockTotal;

    private int spawnedEnemytotal;
    private int blocks;
    private int blockTotal;
    private int spawnedBlockTotal = 0;
    private EnemyDefinition[] enemies;
    public float SetupTimeRemaining { get; private set; }

    // GameController info
    // public int WaveCount => waves.Length;
    public int WaveCount = 10;
    public float SetupDuration;
    // public float SetupDuration => waves[waveNum].startTime;
    public int TotalThisWave { get; private set; }
    public int SpawnedThisWave { get; private set; }

    // Distinct enemy types across all waves
    public EnemyDefinition[] enemyTypes =>
        spawnBlocks
            .Where (bl => bl != null && bl.enemyType != null)
            .Select(bl => bl.enemyType)
            .Distinct()
            .ToArray();
    private float difficultyBudget;

    // Start is called before the first frame update
    void Start()
    {
        baseTarget = GameObject.FindWithTag("Base");
        ResetSpawner();
    }

    // Update is called once per frame
    void Update()
    {
        if (GameController.Instance.HasGameStarted && !GameController.Instance.IsGameOver)
        {
            // If in setup time, decrease time remaining and return
            if (InSetup())
            {
                SetupTimeRemaining -= Time.deltaTime;
                return;
            }

            // If Game has started but wave hasn't call for wave start
            if (!GameController.Instance.HasWaveStarted)
            {
                GameController.Instance.StartWave();
            }
            //If currently spawning a block or have finished spawning the wave, don't do anything
            if (FinishedSpawning || spawning) return;
            //Start Coroutine. Allows enemies to spawn and initialise pauses without interrupting Update() method and general game flow.
            StartCoroutine(SpawnEnemies());
        }
    }

    public void ResetSpawner()
    {
        waveNum = GameController.Instance.currentWave;
        spawnedBlockTotal = 0;
        SpawnedThisWave = 0;
        difficultyBudget = GetWaveDifficulty(waveNum);
        // enemies = GetWaveSpawnBlocksRandom(difficultyBudget);
        // enemies = GetWaveSpawnBlocksSmart(difficultyBudget);
        enemies = GetWaveEnemies(difficultyBudget);
        blockTotal = enemies.Length;
        TotalThisWave = enemies.Length;
    }

    public float SkipSetup()
    {
        float skipped = Mathf.Max(0f, SetupTimeRemaining);
        SetupTimeRemaining = 0f;
        return skipped;
    }

    public void GetComposition(List<(EnemyDefinition def, int count)> into)
    {
        into.Clear();
        if (enemies == null) return;

        foreach (var grp in enemies
            .Where(e => e != null)
            .GroupBy(e => e))
        {
            into.Add((
                grp.Key, 
                grp.Count()
            ));            
        }

    }

    // Check if in Setup stage, StartWave if not already
    private bool InSetup()
    {
        if (SetupTimeRemaining > 0)
        {
            return true;
        }
        else if (GameController.Instance.HasWaveStarted == false)
        {
            GameController.Instance.StartWave();
        }
        return false;

    }


    // IEnumerator allows the method to return pauses for n amount of time. SpawnEnemies calls method SpawnEnemy on all enemies in block.
    // IEnumerator SpawnEnemies()
    // {
    //     spawning = true; //Indicate that spawning has started so that we don't make more coroutines
    //     // For all blocks

    //     for (int i = 0; i < enemies.Length; i++)
    //     {
    //         // Spawn the enemy type of the current block for the amount specified. Call for delay betwwen spawns
    //         SpawnEnemy(enemies[i].enemyType);
    //         for (int j = 1; j< enemies[i].enemyAmount; j++)
    //         {
    //             yield return new WaitForSeconds(enemies[i].delayBetween);
    //             SpawnEnemy(enemies[i].enemyType);
    //         }
    //         // Track total spawned blocks and incur delay between spawn groups
    //         spawnedBlockTotal += 1;
    //         yield return new WaitForSeconds(enemies[i].delayAfterGroup);
    //     }
    //     spawning = false;
    // }
    IEnumerator SpawnEnemies()
    {
        spawning = true; //Indicate that spawning has started so that we don't make more coroutines
        // For all blocks

        for (int i = 0; i < enemies.Length; i++)
        {
            SpawnEnemy(enemies[i]);
            spawnedBlockTotal++;
            yield return new WaitForSeconds(0.5f);
        }
        spawning = false;
    }


    // Spawn enemy of type e
    private void SpawnEnemy(EnemyDefinition e)
    {
        EnemyController enemy = Instantiate(enemyPrefab, transform.position, Quaternion.identity);
        SpawnedThisWave++;
        enemy.Init(e);
        enemy.SetTargetDestination(baseTarget.transform.position);
    }

    private float GetWaveDifficulty(float waveNumber)
    {
        return 55f * Mathf.Pow(1f + 0.15f, waveNumber);
    }

    private float GetEnemyDifficultyCost(EnemyDefinition enemy)
    {
        float speedCost = enemy.speed * 1.0f;
        float healthCost = enemy.maxHealth * 3.0f;
        float damageCost = enemy.damageToBase *1.0f;
        return speedCost + healthCost + damageCost;
    }

    private float GetSpawnBlockCost(EnemySpawnBlockDefinition block)
    {
        if (block == null || block.enemyType == null)
        {
            return 0f;
        }
        return GetEnemyDifficultyCost(block.enemyType) * block.enemyAmount;
    }

    private EnemySpawnBlockDefinition[] GetWaveSpawnBlocksRandom(float difficultyBudget)
    {
        List<EnemySpawnBlockDefinition> generatedBlocks = new List<EnemySpawnBlockDefinition>();

        // Copying into new list so we can get rid of bad ones, not filtering out too expensive ones yet
        var availableBlocks = spawnBlocks
            .Where(bl => bl != null && bl.enemyType != null && bl.enemyAmount > 0)
            .Select(bl => new
            {
                block = bl,
                cost = GetSpawnBlockCost(bl)
            })
            .ToList();

        float remainingBudget = difficultyBudget;

        while (availableBlocks.Count > 0)
        {
            var affordableBlocks = availableBlocks
            .Where(bl => bl.cost <= remainingBudget)
            .ToList();

            if (affordableBlocks.Count == 0)
            {
                Debug.Log("No available blocks to spawn");
                break;
            }

            var selected = affordableBlocks[
                Random.Range(0, affordableBlocks.Count)
            ];

            generatedBlocks.Add(selected.block);

            remainingBudget -= selected.cost;
        }
        // float totalCost = generatedBlocks.Sum(b => GetSpawnBlockCost(b));
        // Debug.Log($"Wave: {waveNum + 1}");
        // Debug.Log($"Difficulty budget: {difficultyBudget}");
        // Debug.Log($"Difficulty spent: {totalCost}");
        // Debug.Log($"Remaining budget: {difficultyBudget - totalCost}");
        // Debug.Log($"Generated blocks: {generatedBlocks.Count}");

        return generatedBlocks.ToArray();
    }


    // Always goes for most difficult block and organises them in ascending difficulty
    private EnemySpawnBlockDefinition[] GetWaveSpawnBlocksSmart(float difficultyBudget)
    {
        List<EnemySpawnBlockDefinition> generatedBlocks = new List<EnemySpawnBlockDefinition>();

        // Copying into new list so we can get rid of bad ones, not filtering out too expensive ones yet
        var availableBlocks = spawnBlocks
            .Where(bl => bl != null && bl.enemyType != null && bl.enemyAmount > 0)
            .Select(bl => new
            {
                block = bl,
                cost = GetSpawnBlockCost(bl)
            })
            .OrderByDescending(bl => bl.cost)
            .ToList();

        float remainingBudget = difficultyBudget;

        while (availableBlocks.Count > 0)
        {
            var selected = availableBlocks.FirstOrDefault(bl => bl.cost <= remainingBudget);

            if (selected == null)
            {
                Debug.Log("No available blocks to spawn");
                break;
            }

            generatedBlocks.Add(selected.block);

            remainingBudget -= selected.cost;
        }

        generatedBlocks.Reverse();

        return generatedBlocks.ToArray();
    }

    private class EnemyGroup
    {
        public EnemyDefinition enemy;
        public int groupSize;
        public float groupCost;
        public float amountSpentOnFaction;
    }

    private EnemyDefinition[] GetWaveEnemies(float difficultyBudget)
    {
        List<EnemyDefinition> generatedEnemies = new();


        var availableEnemies = enemyTypes
            .Where(e => e != null && waveNum >= e.appearWave)
            .Select(e => new EnemyGroup
            {
                enemy = e,
                groupSize = GetGroupSize(e),
                groupCost = GetGroupSize(e) * GetEnemyDifficultyCost(e),
                amountSpentOnFaction = 0
            })
            .ToList();

        // Even Distribution
        float maxFactionPercentage = (float)1f / availableEnemies.Count;
        float remainingBudget = difficultyBudget;
        
        while (availableEnemies.Count > 0)
        {
            var affordableEnemies = availableEnemies
                .Where(e => e.groupCost <= remainingBudget && e.amountSpentOnFaction / difficultyBudget < maxFactionPercentage)
                .ToList();
            if (affordableEnemies.Count == 0)
            {
                // Find max spawnable enemies within budget here
                break;
            }
            var selected = affordableEnemies[Random.Range(0, affordableEnemies.Count)];


            for (int i = 0; i < GetGroupSize(selected.enemy); i++)
            {
                generatedEnemies.Add(selected.enemy);
            }
            remainingBudget -= selected.groupCost;
            selected.amountSpentOnFaction += selected.groupCost;
        }

        return generatedEnemies.ToArray();
    }

            //     var selected = affordableBlocks[
            //     Random.Range(0, affordableBlocks.Count)
            // ];

    private int GetGroupSize(EnemyDefinition enemy)
    {
        if (WaveCount <= 1) return enemy.maxGroupSize;

        float progress = (float)(waveNum - 1) / (WaveCount - 1);
        return Mathf.RoundToInt(Mathf.Lerp(enemy.minGroupSize, enemy.maxGroupSize, progress));
    }
}
    

