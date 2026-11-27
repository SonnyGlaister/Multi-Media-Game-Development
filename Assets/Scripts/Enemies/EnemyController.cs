using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class EnemyController : MonoBehaviour
{
    public static readonly List<EnemyController> Alive = new();

    public Image RemainingHealth;
    public Renderer bodyRenderer;

    [Header("Knockback")]
    [Tooltip("How fast a knockback pushes this enemy back (units per second).")]
    public float knockbackSpeed = 12f;

    public EnemyDefinition Definition { get; private set; }
    public FactionDefinition Faction => Definition != null ? Definition.faction : null;

    public float RemainingDistance;
    public bool IsAlive { get; private set; } = true;
    public int MaxHealth => Definition != null ? Mathf.Max(1, Definition.maxHealth) : 1;
    public int Health => health;
    public bool IsKnockedBack => knockRemaining > 0f;

    private int health;
    private NavMeshAgent agent;

    // Knockback
    private Vector3[] route;
    private float[] routeDistance;
    private float knockRemaining;
    private Vector3 fallbackDirection;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void OnEnable() => Alive.Add(this);
    private void OnDisable() => Alive.Remove(this);

    public void Init(EnemyDefinition definition)
    {
        Definition = definition;
        name = definition.displayName;
        health = definition.maxHealth;
        transform.localScale = Vector3.one * definition.scale;
        agent.speed = definition.speed;

        if (definition.modelPrefab != null)
        {
            if (bodyRenderer != null) bodyRenderer.enabled = false;
            Instantiate(definition.modelPrefab, transform);
        }
        else if (bodyRenderer != null)
        {
            bodyRenderer.material.color = definition.Tint;
        }
    }

    private void Update()
    {
        if (!IsAlive || agent == null) return;
        if (!agent.isOnNavMesh) return;

        if (route == null && !agent.pathPending && agent.hasPath) SaveRoute();

        if (knockRemaining > 0f)
        {
            UpdateKnockback();
            return;
        }

        if (agent.pathPending || !agent.hasPath) return;

        RemainingDistance = agent.remainingDistance;
        if (RemainingDistance <= agent.stoppingDistance)
        {
            ReachEnd();
        }
    }

    public void TakeDamage(int amount)
    {
        if (!IsAlive || amount <= 0) return;

        health -= amount;
        if (RemainingHealth != null)
            RemainingHealth.fillAmount = Mathf.Clamp01((float)health / MaxHealth);

        if (health <= 0)
        {
            IsAlive = false;
            GameController.Instance.EnemyKilled(this);
            Destroy(gameObject);
        }
    }

    public void ReachEnd()
    {
        if (!IsAlive) return;
        IsAlive = false;
        GameController.Instance.EnemyReachedBase(Definition);
        Destroy(gameObject);
    }

    public void SetTargetDestination(Vector3 target)
    {
        agent.SetDestination(target);
        if (!agent.isOnNavMesh)
        {
            Debug.LogError("Enemy is not on the NavMesh");
        }
    }

    public static List<EnemyController> InRadius(Vector3 center, float radius)
    {
        List<EnemyController> found = new();
        foreach (EnemyController e in Alive)
        {
            if (!e.IsAlive) continue;
            Vector3 d = e.transform.position - center;
            d.y = 0f;
            if (d.sqrMagnitude <= radius * radius) found.Add(e);
        }
        return found;
    }

    // Knockback (Mechanic faction)

    public void KnockBack(float distance)
    {
        if (!IsAlive || distance <= 0f || !agent.isOnNavMesh) return;

        float resistance = Definition != null ? Definition.knockbackResistance : 0f;
        distance *= 1f - Mathf.Clamp01(resistance);
        if (distance <= 0.01f) return;

        if (route == null)
        {
            Vector3 v = agent.velocity;
            v.y = 0f;
            if (v.sqrMagnitude < 0.01f) return;
            fallbackDirection = -v.normalized;
        }

        knockRemaining = Mathf.Max(knockRemaining, distance);

        agent.isStopped = true;
        agent.velocity = Vector3.zero;
    }

    private void UpdateKnockback()
    {
        float step = Mathf.Min(knockRemaining, knockbackSpeed * Time.deltaTime);
        knockRemaining -= step;

        Vector3 position = transform.position;
        Vector3 target = route != null
            ? RoutePointAt(RouteProgress(position) - step)
            : position + fallbackDirection * step;

        Vector3 move = target - position;
        move.y = 0f;
        agent.Move(move);

        if (knockRemaining <= 0f)
        {
            knockRemaining = 0f;
            agent.isStopped = false;
        }
    }

    private void SaveRoute()
    {
        Vector3[] corners = agent.path.corners;
        if (corners.Length < 2) return;

        route = corners;
        routeDistance = new float[route.Length];
        for (int i = 1; i < route.Length; i++)
            routeDistance[i] = routeDistance[i - 1] + Flat(route[i] - route[i - 1]).magnitude;
    }

    private float RouteProgress(Vector3 p)
    {
        Vector3 flatP = Flat(p);
        float bestSqr = float.MaxValue;
        float progress = 0f;

        for (int i = 0; i < route.Length - 1; i++)
        {
            Vector3 a = Flat(route[i]);
            Vector3 ab = Flat(route[i + 1]) - a;
            float lengthSqr = ab.sqrMagnitude;
            if (lengthSqr < 0.0001f) continue;

            float t = Mathf.Clamp01(Vector3.Dot(flatP - a, ab) / lengthSqr);
            float sqr = (flatP - (a + ab * t)).sqrMagnitude;
            if (sqr < bestSqr)
            {
                bestSqr = sqr;
                progress = routeDistance[i] + Mathf.Sqrt(lengthSqr) * t;
            }
        }
        return progress;
    }

    private Vector3 RoutePointAt(float distance)
    {
        distance = Mathf.Max(0f, distance);
        for (int i = 0; i < route.Length - 1; i++)
        {
            float segment = routeDistance[i + 1] - routeDistance[i];
            if (distance <= routeDistance[i + 1] && segment > 0.0001f)
                return Vector3.Lerp(route[i], route[i + 1], (distance - routeDistance[i]) / segment);
        }
        return route[route.Length - 1];
    }

    private static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0f, v.z);

    // Remaining path distance to HQ (smaller = closer)
    public float GetDistanceAlongPath()
    {
        Vector3[] corners = agent.path.corners;
        if (corners.Length < 2) return 0f;

        Vector3 position = agent.nextPosition;
        float distance = 0f;

        for (int i = 0; i < corners.Length - 1; i++)
        {
            Vector3 a = corners[i];
            Vector3 b = corners[i + 1];
            Vector3 segment = b - a;

            float t = Mathf.Clamp01(Vector3.Dot(position - a, segment) / segment.sqrMagnitude);
            Vector3 closest = a + t * segment;

            if (Vector3.Distance(position, closest) < 0.5f)
            {
                distance += Vector3.Distance(a, closest);
                for (int j = i + 1; j < corners.Length - 1; j++)
                    distance += Vector3.Distance(corners[j], corners[j + 1]);
                return distance;
            }

            distance += segment.magnitude;
        }

        return distance;
    }
}
