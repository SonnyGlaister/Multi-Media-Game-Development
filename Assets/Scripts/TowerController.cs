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

    public int startCost = 5;

    public int currentCost { get; private set; }
    private float TOWERCOSTMULTIPLIER = 2.0f; //Multiplier to increase tower cost by

    private int placed;

    private float timer;
    private EnemyController currentTarget;
    public static bool redPerkPurchased = false;
    public static bool yellowPerkPurchased = false;
    public static bool redPerkPurchased1 = false;
    public static bool yellowPerkPurchased1 = false;

    

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

        if (redPerkPurchased & !redPerkPurchased1)
        {
            cooldown = 0.9f;
        } else if (redPerkPurchased1)
        {
            cooldown = 0.75f;
        }

        if (yellowPerkPurchased & !yellowPerkPurchased1)
        {
            range = 6;
        } else if (yellowPerkPurchased1)
        {
            range = 8;
        }
    }

    // Reset costs and placed totals.
    public void Reset()
    {
        placed = 0;
        currentCost = startCost;
    }
    // Update the cost based on total placed.
    public void updateCost()
    {
        placed += 1;
        // First 2 towers cost the same
        if (placed < 2)
        {
            currentCost = startCost;
        }
        else
        {
            currentCost = (int)(currentCost * TOWERCOSTMULTIPLIER);
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
