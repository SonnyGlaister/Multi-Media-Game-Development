using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject[] enemyPrefabs; //Insert multiple enemy types
    public float setupTime = 5;
    public int[] enemySpawnNumbers; //Set amounts of enemy types
    public float enemySpawnCooldown;
    private int enemySpawnNumber = 0;
    public float SetupTimeRemaining { get; private set; }
    public bool FinishedSpawning => spawnedEnemyTotal >= enemySpawnNumber;

    private int spawnedEnemyTotal;
    private float timer;



    // Start is called before the first frame update
    void Start()
    {
        foreach (int num in enemySpawnNumbers) //Total enemies to spawn stored
        {
            enemySpawnNumber += num;
        }
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

            if (FinishedSpawning) return;

            timer += Time.deltaTime;
            if (timer >= enemySpawnCooldown)
            {
                int select = 0;
                for (int i = 0; i < enemySpawnNumbers.Length; i++)
                {
                    if (enemySpawnNumbers[i] != 0)
                    {
                        select = i;
                        break;
                    }
                }
                Instantiate(enemyPrefabs[select], transform.position, Quaternion.identity);
                spawnedEnemyTotal++;
                enemySpawnNumbers[select] -= 1;
                timer = 0;
            }

        }
    }
}