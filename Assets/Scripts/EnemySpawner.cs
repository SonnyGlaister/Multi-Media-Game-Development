using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;


public class EnemySpawner : MonoBehaviour
{
    public GameObject[] enemyPrefabs; //Insert multiple enemy types
    public float setupTime = 3; //Time between waves
    public int[][] enemySpawnNumbers =
    {
        new int[] {10,-1, -1},
        new int[] {8,6, -1},
        new int[] {8,10, -1},
        new int[] {2, 15, -1},
        new int[] {20, 20, 1}
    }; //Set amounts of enemy types for all waves. If -1, cannot be randomly spawned in wave.

    public float enemySpawnCooldown; // Time between enemy spawns in a wave

    private int enemySpawnNumber = 0;
    public float SetupTimeRemaining { get; private set; }
    public bool FinishedSpawning => spawnedEnemyTotal >= enemySpawnNumber;

    private int spawnedEnemyTotal;
    private float timer;
    private int waveNum = 0;

    private float RANDOMDIVISOR = 2.2F;

    private int[][] randomisedEnemySpawnNumbers;



    // Start is called before the first frame update
    void Start()
    {
        Random.InitState(System.DateTime.Now.Millisecond); //Initialise random seed

        randomisedEnemySpawnNumbers = cloneJaggedArray(enemySpawnNumbers);

        waveNum = GameController.Instance.currentWave;
        SetupTimeRemaining = setupTime;
        getTotalEnemies();
        timer = enemySpawnCooldown;

        if (waveNum > 0) standardRandomisation();
    }

    // Update is called once per frame
    void Update()
    {
        if (GameController.Instance.HasGameStarted)
        {
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
                Instantiate(enemyPrefabs[select], transform.position, Quaternion.identity);
                spawnedEnemyTotal++;
                randomisedEnemySpawnNumbers[waveNum][select] -= 1;
                timer = 0;
            }

        }
    }

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

    //Called before every round (besides 0).
    public void ResetSpawner()
    {
        waveNum = GameController.Instance.currentWave;
        spawnedEnemyTotal = 0;
        SetupTimeRemaining = setupTime;
        timer = enemySpawnCooldown;
        standardRandomisation();
    }


    //Logarithmic wave randomisation
    private void standardRandomisation()
    {

        getTotalEnemies(); //Instantiate current enemySpawnNumber for future calculation
        int l = randomisedEnemySpawnNumbers[waveNum].Length;

        float randomChance = Mathf.Log10(waveNum);
        List<int> enemyTypes = new List<int>();

        randomChance = randomChance / RANDOMDIVISOR;

        if (randomChance > 0.7)
        {
            randomChance = 0.7F;
        }


        

        for (int j = 0; j < l; j++)
        {
            if (randomisedEnemySpawnNumbers[waveNum][j] != -1 && randomisedEnemySpawnNumbers[waveNum][j] != 1)
            {
                enemyTypes.Add(j);
            }
        }

        // Defined percentage of randomisation to wave

        int totalRandomised = (int)(randomChance * enemySpawnNumber);

        for (int i = 0; i < totalRandomised; i++)
        {
            // Take the standard wave format and mutate totalRandomised amount of enemies

            int r1 = enemyTypes[Random.Range(0, enemyTypes.Count)];
            randomisedEnemySpawnNumbers[waveNum][r1] -= 1;

            int r2 = enemyTypes[Random.Range(0, enemyTypes.Count)];
            randomisedEnemySpawnNumbers[waveNum][r2] += 1;

        }

    }

    // Return total enemies to spawn during the upcoming wave
    private void getTotalEnemies()
    {
        enemySpawnNumber = 0;
        int sum = 0;
        foreach (int num in randomisedEnemySpawnNumbers[waveNum]) //Total enemies to spawn this wave stored
        {
            if (num == -1)
            {
                continue;
            }
            sum += num;
        }
        enemySpawnNumber = sum;
    }

    // Used to clone original enemy spawn numbers array before randomisation.
    public int[][] cloneJaggedArray(int[][] original)
    {
        if (original == null)
        {
            return null;
        }

        int[][] clone = new int[original.Length][];

        for (int i = 0; i < original.Length; i++)
        {
            if (original[i] != null)
            {
                clone[i] = new int[original[i].Length];
                for (int j = 0; j < original[i].Length; j++)
                {
                    clone[i][j] = original[i][j];
                }
            }
            else
            {
                clone[i] = null;
            }
        }
        return clone;

    }

}