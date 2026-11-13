using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System;
using System.Linq;
using System.Linq.Expressions;

public class EnemySpawner2 : MonoBehaviour
{
    public EnemyController enemyPrefab;
    public WaveDefinition[] waves;
    public int waveNum { get; private set; }

    private GameObject baseTarget;
    private bool spawning=false;

    public bool FinishedSpawning => spawnedBlockTotal >= blockTotal;

    private int spawnedEnemytotal;
    private int blocks;
    private int blockTotal;
    private int spawnedBlockTotal = 0;
    private EnemySpawnBlockDefinition[] enemies;
    public float SetupTimeRemaining { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        waveNum = GameController.Instance.currentWave;
        blocks = waves[waveNum].enemies.Length;
        blockTotal = blocks;
        SetupTimeRemaining = waves[waveNum].startTime; //Store and decrease the wave's unique setup time
        baseTarget = GameObject.FindWithTag("Base");
        GameController.Instance.setTotalWaves(waves.Length);
    }

    // Update is called once per frame
    void Update()
    {
        if (GameController.Instance.HasGameStarted)
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
        SetupTimeRemaining = waves[waveNum].startTime; //Store and decrease the wave's unique setup time
        blocks = waves[waveNum].enemies.Length;
        int blockTotal = blocks;
        baseTarget = GameObject.FindWithTag("Base");
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
    IEnumerator SpawnEnemies()
    {

        enemies = waves[waveNum].enemies;
        spawning = true; //Indicate that spawning has started so that we don't make more coroutines
        // For all blocks
        for (int i = 0; i < enemies.Length; i++)
        {
            // Spawn the enemy type of the current block for the amount specified. Call for delay betwwen spawns
            SpawnEnemy(enemies[i].enemyType);
            for (int j = 1; j< enemies[i].enemyAmount; j++)
            {
                yield return new WaitForSeconds(enemies[i].delayBetween);
                SpawnEnemy(enemies[i].enemyType);
            }
            // Track total spawned blocks and incur delay between spawn groups
            spawnedBlockTotal += 1;
            yield return new WaitForSeconds(enemies[i].delayAfterGroup);
        }
        spawning = false;
    }

    // Spawn enemy of type e
    private void SpawnEnemy(EnemyDefinition e)
    {
        EnemyController enemy = Instantiate(enemyPrefab, transform.position, Quaternion.identity);
        enemy.Init(e);
        enemy.SetTargetDestination(baseTarget.transform.position);
    }
}
    

