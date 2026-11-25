using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ExplodeOnDeathBehaviour : MonoBehaviour
{
    public int damage = 2;
    public float radius = 3;
    public bool friendlyFire = false;
    public GameObject explosionEffect;

    private bool hasExploded = false; 
    private EnemyController enemy;

    private void Awake()
    {
        // Get EnemyController from current component of which this attribute is attached to.
        enemy = GetComponent<EnemyController>();
    }

    private void OnEnable()
    {
       if (enemy != null)
        {
            enemy.Died += this.OnDeath;
        }

    }

    private void OnDisable()
    {
        if (enemy != null)
        {
            enemy.Died -= this.OnDeath;
        }
    }

    private void OnDeath(EnemyController enemy)
    {
        if (hasExploded)
        {
            return;
        }

        hasExploded = true;
        Explode();
    }

    private void Explode()
    {
        //Inflict damage in radius sphere
        Collider[] ObjectsInRadius = Physics.OverlapSphere(enemy.transform.position, radius);

        // If we are damaging other enemies
        if (friendlyFire)
        {
            foreach (EnemyController e in GetEnemiesInExplosion(enemy.transform.position, ObjectsInRadius))
            {
                e.TakeDamage(damage);
            }
        }

        // If enemy did not reach base (auto applied damage to base)
        if (! (enemy.RemainingDistance <= enemy.agent.stoppingDistance))
        {
            for (int i = 0; i < ObjectsInRadius.Length; i++)
            {
                if (ObjectsInRadius[i].gameObject.tag == "Base")
                {
                    // Need to change damage used here from enemy damage to explosion damage
                    GameController.Instance.EnemyReachedBase(enemy.Definition);
                }
            }

        }

        // Play animation (or project 3d mesh image) of explosion
        
        // Add Lighting to explosion

    }

    // Implementation taken from ProjectileController
    private EnemyController[] GetEnemiesInExplosion(Vector3 position, Collider[] ObjectsInRadius)
    {
        EnemyController[] enemies = ObjectsInRadius.Where(entity => entity.CompareTag("Enemy"))
            .Select(entity => entity.GetComponent<EnemyController>())
            .Where(enemyController => enemyController != null)
            .ToArray();

        return enemies;
    }


}
