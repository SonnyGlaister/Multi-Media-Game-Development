using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using TMPro;
using UnityEngine;


public class EnemySpawner : MonoBehaviour
{
    public GameObject[] enemyPrefabs; //Insert multiple enemy types
    public bool randomise = true; //Include logarithmic randomisation to wave enemies
    public float setupTime = 3;
    public int[][] enemySpawnNumbers =
    {
        new int[] {10,-1},
        new int[] {8,6},
        new int[] {8,10},
        new int[] {2, 15},
        new int[] {20, 20}
    }; //Set amounts of enemy types for all waves. If -1, cannot be randomly spawned in wave.

    public float enemySpawnCooldown;
    private int enemySpawnNumber = 0;
    public float SetupTimeRemaining { get; private set; }
    public bool FinishedSpawning => spawnedEnemyTotal >= enemySpawnNumber;

    private int spawnedEnemyTotal;
    private float timer;
    private int waveNum=0;

    private float RANDOMDIVISOR = 2.2F;



    // Start is called before the first frame update
    void Start()
    {
        Random.InitState(System.DateTime.Now.Millisecond);
        if (randomise && waveNum > 0) standardRandomisation();

        foreach (int num in enemySpawnNumbers[waveNum]) //Total enemies to spawn this wave stored
        {
            if (num == -1)
            {
                continue;
            }
            enemySpawnNumber += num;
        }
        waveNum = GameController.Instance.currentWave;
        SetupTimeRemaining = setupTime;
        timer = enemySpawnCooldown;

    }

    // Update is called once per frame
    void Update()
    {
        if (GameController.Instance.HasGameStarted)
        {
            if (SetupTimeRemaining > 0)
            {
                SetupTimeRemaining -= Time.deltaTime;
                return;
            }

            if (! GameController.Instance.HasWaveStarted)
            {
                GameController.Instance.StartWave();
            }


            if (FinishedSpawning) return;

            timer += Time.deltaTime;
            if (timer >= enemySpawnCooldown)
            {
                int select = 0;
                for (int i = 0; i < enemySpawnNumbers[waveNum].Length; i++)
                {
                    if (enemySpawnNumbers[waveNum][i] != 0)
                    {
                        select = i;
                        break;
                    }
                }
                Instantiate(enemyPrefabs[select], transform.position, Quaternion.identity);
                spawnedEnemyTotal++;
                enemySpawnNumbers[waveNum][select] -= 1;
                timer = 0;
            }

        }
    }

    public void ResetSpawner()
    {
        waveNum = GameController.Instance.currentWave;
        spawnedEnemyTotal = 0;
        int sumEnemy = 0;
        foreach (int num in enemySpawnNumbers[waveNum]) //Total enemies to spawn this wave stored
        {
            sumEnemy += num;
        }
        enemySpawnNumber = sumEnemy;
        if (randomise) standardRandomisation();
        SetupTimeRemaining = setupTime;
        timer = enemySpawnCooldown;
    }

    //Logarithmic wave randomisation
    private void standardRandomisation()
    {
        int l = enemySpawnNumbers[waveNum].Length;

        float randomChance = Mathf.Log10(waveNum);
        randomChance = randomChance / RANDOMDIVISOR;

        if (randomChance > 0.7)
        {
            randomChance = 0.7F;
        }


        List<int> enemyTypes = new List<int>();

        for (int j = 0; j < l; j++)
        {
            if (enemySpawnNumbers[waveNum][j] != -1)
            {
                enemyTypes.Add(j);
            }
        }

        // Defined percentage of randomisation to wave

        int totalRandomised = (int)(randomChance * enemySpawnNumber);

        for (int i = 0; i < totalRandomised; i++)
        {
            // Take the standard wave format and mutate totalRandomised amount of enemies

            int r1 = enemyTypes[Random.Range(0,enemyTypes.Count)];        
            enemySpawnNumbers[waveNum][r1] -= 1;

            int r2 = enemyTypes[Random.Range(0, enemyTypes.Count)];
            enemySpawnNumbers[waveNum][r2] += 1;

        }

    }
    
}