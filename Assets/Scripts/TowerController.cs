using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerController : MonoBehaviour
{
    public static float cooldown = 1f;
    public float range = 8;
    public int damage = 1;
    public Rigidbody projectile;
    public float projectileSpeed = 20;

    public int cost = 5;

    private float timer;
    private EnemyController currentTarget;
    public static bool redPerkPurchased = false;
    public static bool yellowPerkPurchased = false;

    void Update()
    {
        timer += Time.deltaTime;
        currentTarget = FindTarget();

        if (currentTarget != null)
        {
            // Debug: red line to the current target
            Debug.DrawLine(transform.position, currentTarget.transform.position, Color.red);

            // We should add debug to show radius of towers
            if (timer >= cooldown)
            {
                Shoot(currentTarget);
                timer = 0f;
            }
        }

        if (redPerkPurchased)
        {
            cooldown = 0.5f;
        } else
        {
            cooldown = 1f;
        }

        if (yellowPerkPurchased)
        {
            range = 16;
        } else
        {
            range = 8;
        }
    }

    // Picks the enemy in range that is furthest along the path
    EnemyController FindTarget()
    {
        EnemyController best = null;
        float bestProgress = -1f;

        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            if (go == null) continue;

            EnemyController enemy = go.GetComponent<EnemyController>();
            // if (enemy == null || !enemy.IsAlive) continue;
            if (enemy == null) continue;
            if (!enemy.IsAlive) continue;

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
