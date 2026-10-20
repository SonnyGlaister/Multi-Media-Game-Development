using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using TMPro;
using UnityEngine;


public class EnemySpawner : MonoBehaviour
{
    public GameObject[] enemyPrefabs; //Insert multiple enemy types
    public bool randomise = false; //Randomise no. each enemy type
    public float setupTime = 3;
    public int[][] enemySpawnNumbers =
    {
        new int[] {10,0},
        new int[] {8,6},
        new int[] {8,10},
        new int[] {2, 15},
        new int[] {20, 20}
    }; //Set amounts of enemy types for all waves

    public float enemySpawnCooldown;
    private int enemySpawnNumber = 0;
    public float SetupTimeRemaining { get; private set; }
    public bool FinishedSpawning => spawnedEnemyTotal >= enemySpawnNumber;

    private int spawnedEnemyTotal;
    private float timer;
    private int waveNum=0;



    // Start is called before the first frame update
    void Start()
    {
        if (randomise && waveNum > 0) randomEnemies();

        foreach (int num in enemySpawnNumbers[waveNum]) //Total enemies to spawn this wave stored
        {
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

    //Reset and update Spawner variables for each wave
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
        SetupTimeRemaining = setupTime;
        timer = enemySpawnCooldown;
    }

    // Randomise division of total enemies between input enemy types
    private void randomEnemies()
    {
        for (int i = 0; i < enemySpawnNumbers.Length; i++)
        {
            int total = 0;
            for (int j = 0; j < enemySpawnNumbers[i].Length; j++)
            {
                total += enemySpawnNumbers[i][j];
            }

            for (int k = 0; k < enemySpawnNumbers[i].Length; k++)
            {
                if (k == enemySpawnNumbers[i].Length -1)
                {
                    enemySpawnNumbers[i][k] = total;
                    break;
                }
                enemySpawnNumbers[i][k] = Random.Range(0, total + 1);
                total -= enemySpawnNumbers[i][k];
            }
        }
    }
}