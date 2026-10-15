using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public GameController gameController;
    public float setupTime = 5;
    public int enemySpawnNumber = 5;
    public float enemySpawnCooldown;

    public float SetupTimeRemaining { get; private set; }
    public bool FinishedSpawning => spawnedEnemyTotal >= enemySpawnNumber;

    private int spawnedEnemyTotal;
    private float timer;



    // Start is called before the first frame update
    void Start()
    {
        SetupTimeRemaining = setupTime;
        timer = enemySpawnCooldown;
    }

    // Update is called once per frame
    void Update()
    {
        if (gameController.HasGameStarted)
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
                Instantiate(enemyPrefab, transform.position, Quaternion.identity);
                spawnedEnemyTotal++;
                timer = 0;
            }
        }

    }
}
