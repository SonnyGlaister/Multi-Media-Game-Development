using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ProjectileController : MonoBehaviour
{
    public int damage = 1;
    public float speed = 20;
    public int piercing = 0;
    public float aoeRange = 0f;
    public float aoeDamage = 0f;
    public float lifetime = 3;

    private Rigidbody rb;
    private Transform target;

    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void Launch(Transform newTarget, int newDamage, float newSpeed, int newPiercing, float newAOERange, float newAOEDamage)
    {
        rb = GetComponent<Rigidbody>();
        target = newTarget;
        damage = newDamage;
        speed = newSpeed;
        piercing = newPiercing;
        aoeRange = newAOERange;
        aoeDamage = newAOEDamage;
        if (target != null)
        {
            rb.velocity = (target.position - transform.position).normalized * speed;
        }
    }

    void FixedUpdate()
    {
        // Steer towards the target each step; if it died, keep flying straight.
        if (target != null)
        {
            rb.velocity = (target.position - transform.position).normalized * speed;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        EnemyController enemy = other.GetComponent<EnemyController>();
        if (enemy == null || !enemy.IsAlive) return;

   
        if (aoeRange > 0)
        {
            // for GetEnemiesInRadius(enemy.transform.position);
            foreach (EnemyController e in GetEnemiesInRadius(enemy.transform.position, aoeRange))
            {
                e.TakeDamage(Mathf.RoundToInt(damage * aoeDamage));
            }
        } else
        {
            enemy.TakeDamage(damage);
        }

        if (piercing > 0)
        {
            piercing -= 1;
        } else
        {
            Destroy(gameObject);
        }

    }

    EnemyController[] GetEnemiesInRadius(Vector3 position, float radius)
    {
        Collider[] ObjectsInRadius = Physics.OverlapSphere(position, radius);

        EnemyController[] enemies = ObjectsInRadius
        // .Where(entity => entity.CompareTag("Enemy")) 
        .Select(entity => entity.GetComponent<EnemyController>())
        .Where(enemyController => enemyController != null)
        .ToArray();

        return enemies;
    }
}
