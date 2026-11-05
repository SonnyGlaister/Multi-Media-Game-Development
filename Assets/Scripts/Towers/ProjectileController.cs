using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    public int damage = 1;
    public float speed = 20;
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


    public void Launch(Transform newTarget, int newDamage, float newSpeed)
    {
        rb = GetComponent<Rigidbody>();
        target = newTarget;
        damage = newDamage;
        speed = newSpeed;
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

        enemy.TakeDamage(damage);
        Destroy(gameObject);
    }
}
