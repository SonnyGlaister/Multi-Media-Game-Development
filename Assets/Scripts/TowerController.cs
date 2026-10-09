using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerController : MonoBehaviour
{
    private GameObject[] enemies;

    public float cooldown = 5;
    float timer = 0;
    private Vector3 towerPosition;
    private GameObject closestEnemy;
    private float closestEnemyDistance;
    public Rigidbody projectile;
    public float projectileSpeed = 20f;
    void Start()
    {
        towerPosition = transform.position;
    }

    // I DON'T KNOW IF I LIKE THIS TIMER IMPLEMENTATION OPEN TO SUGGESTIONS
    void Update()
    {
        enemies = GameObject.FindGameObjectsWithTag("Enemy");
        closestEnemy = null;
        closestEnemyDistance = Mathf.Infinity;

        foreach(GameObject enemy in enemies)
        {
            float distanceToEnemy = Vector3.Distance(towerPosition, enemy.transform.position);

            if (distanceToEnemy < closestEnemyDistance)
            {
                closestEnemyDistance = Vector3.Distance(towerPosition, enemy.transform.position);
                closestEnemy = enemy;
            }
        }

        timer += Time.deltaTime;
        // WE SHOULD IMPLEMENT DEBUG HERE TO SHOW WHAT ENEMIES ARE BEING TARGETED BY CHANGING COLOUR OR SUM
        if (timer >= cooldown && closestEnemy != null)
        {
            onCooldownOver(closestEnemy);
            timer = 0f;
        }

    }

    void onCooldownOver(GameObject target)
    {
        Vector3 projectileDirection = (target.transform.position - towerPosition).normalized;
        // closestEnemy.gameObject.SetActive(false);
        Rigidbody projectileInstance = Instantiate(projectile, transform.position, Quaternion.identity);
        
        
        projectileInstance.velocity = projectileDirection * projectileSpeed;
    }
}
