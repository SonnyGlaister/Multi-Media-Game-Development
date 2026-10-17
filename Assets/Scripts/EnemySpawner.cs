using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject[] enemyPrefabs; //Insert multiple enemy types
    public float setupTime = 5;
    public int[][] enemySpawnNumbers =
    {
        new int[] {6,2},
        new int[] {6,4},
        new int[] {12,10}
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
}