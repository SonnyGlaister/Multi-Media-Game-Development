using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerController : MonoBehaviour
{
    public string displayName = "Tower";
    public Sprite icon;

    public float range = 8;
    public int damage = 1;
    public int piercing = 0;
    public float fireCooldown = 1f;
    public float aoeRange = 0;
    public float aoeDamage = 0;

    // Real stats after perks
    float Cooldown => fireCooldown * PerkManager.Instance.CooldownMultiplier;
    float Range => range * PerkManager.Instance.RangeMultiplier;
    int Damage => damage + PerkManager.Instance.DamageBonus;
    int Piercing => piercing + PerkManager.Instance.PiercingBonus;
    float AOERange => aoeRange + PerkManager.Instance.AOERangeBonus;
    float AOEDamage => aoeDamage + PerkManager.Instance.AOEDamageMultiplier;

    public Rigidbody projectile;
    public float projectileSpeed = 20;

    public int startCost = 5;

    public int currentCost { get; private set; }
    private float TOWERCOSTMULTIPLIER = 2.0f; //Multiplier to increase tower cost by

    private int placed;

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

            // We should add debug to show radius of towers
            if (timer >= Cooldown)
            {
                Shoot(currentTarget);
                timer = 0f;
            }
        }
    }

    // Reset costs and placed totals.
    public void ResetCost()
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
        float bestProgress = Mathf.Infinity;

        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            if (go == null) continue;
            EnemyController enemy = go.GetComponent<EnemyController>();
            if (enemy == null) continue;
            if (!enemy.IsAlive) continue;

            if (Vector3.Distance(transform.position, go.transform.position) > Range) continue;

            float enemyDistance = enemy.GetDistanceAlongPath();

            if (enemyDistance < bestProgress)
            {
                bestProgress = enemyDistance;
                best = enemy;
            }
            // Debug.Log(bestProgress);
        }
        return best;
    }

    void Shoot(EnemyController target)
    {
        Rigidbody instance = Instantiate(projectile, transform.position, Quaternion.identity);
        instance.GetComponent<ProjectileController>().Launch(target.transform, Damage, projectileSpeed, Piercing, AOERange, AOEDamage);
    }
}
