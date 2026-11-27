using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    public float lifetime = 3f;

    private Rigidbody rb;
    private int damage;
    private int piercing;
    private float aoeRadius;

    private void Awake() => rb = GetComponent<Rigidbody>();
    private void Start() => Destroy(gameObject, lifetime);

    public void Launch(Transform target, int newDamage, float speed, int newPiercing, float newAoeRadius)
    {
        damage = newDamage;
        piercing = newPiercing;
        aoeRadius = newAoeRadius;

        Vector3 direction = target != null ? target.position - transform.position : transform.forward;
        if (direction.sqrMagnitude < 0.0001f) direction = transform.forward;
        direction.Normalize();

        rb.velocity = direction * speed;
        transform.forward = direction;
    }

    private void OnTriggerEnter(Collider other)
    {
        EnemyController enemy = other.GetComponentInParent<EnemyController>();
        if (enemy == null || !enemy.IsAlive) return;

        if (aoeRadius > 0f)
            foreach (EnemyController e in EnemyController.InRadius(enemy.transform.position, aoeRadius)) e.TakeDamage(damage);
        else
            enemy.TakeDamage(damage);

        if (piercing > 0) piercing--;
        else Destroy(gameObject);
    }
}