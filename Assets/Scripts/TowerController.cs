using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerController : MonoBehaviour
{
    public float cooldown = 1;
    public float range = 8;
    public int damage = 1;
    public Rigidbody projectile;
    public float projectileSpeed = 20;

    public int cost = 5;

    private float timer;
    private EnemyController currentTarget;

    void Update()
    {
        timer += Time.deltaTime;
        currentTarget = FindTarget();

        if (currentTarget != null)
        {
            // Debug: red line to the current target
            Debug.DrawLine(transform.position, currentTarget.transform.position, Color.red);

            if (timer >= cooldown)
            {
                Shoot(currentTarget);
                timer = 0f;
            }
        }
    }

    // Picks the enemy in range that is furthest along the path
    EnemyController FindTarget()
    {
        EnemyController best = null;
        float bestProgress = -1f;

        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            EnemyController enemy = go.GetComponent<EnemyController>();
            if (enemy == null || !enemy.IsAlive) continue;

            if (Vector3.Distance(transform.position, go.transform.position) > range) continue;

            if (enemy.DistanceTravelled > bestProgress)
            {
                bestProgress = enemy.DistanceTravelled;
                best = enemy;
            }
        }
        return best;
    }

    void Shoot(EnemyController target)
    {
        Rigidbody instance = Instantiate(projectile, transform.position, Quaternion.identity);
        instance.GetComponent<ProjectileController>().Launch(target.transform, damage, projectileSpeed);
    }
}
