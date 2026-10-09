using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public int enemySpawnNumber = 5;
    private int spawnedEnemyTotal;
    public float enemySpawnCooldown;
    private float timer;
    // Start is called before the first frame update
    void Start()
    {
        Instantiate(enemyPrefab, new Vector3(5, 0, 5), Quaternion.identity);
    }

    // Update is called once per frame
    void Update()
    {
        if (timer > enemySpawnCooldown && spawnedEnemyTotal < enemySpawnNumber)
        {
            Instantiate(enemyPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            spawnedEnemyTotal ++;

            timer = 0;
        } else
        {
            timer += Time.deltaTime;
        }

    }
}
