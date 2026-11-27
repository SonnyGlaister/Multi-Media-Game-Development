using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    public static readonly List<EnemyController> Alive = new();

    public Image RemainingHealth;
    public Renderer bodyRenderer;

    public EnemyDefinition Definition { get; private set; }
    public FactionDefinition Faction => Definition != null ? Definition.faction : null;

    public float RemainingDistance;
    public bool IsAlive { get; private set; } = true;
    public int MaxHealth => Definition != null ? Mathf.Max(1, Definition.maxHealth) : 1;

    private int health;
    public NavMeshAgent agent;

    // Event trigger when enemy dies
    public event System.Action<EnemyController> Died;

    // Event trigger when enemy spawns
    public event System.Action<EnemyController> Spawned;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void OnEnable() => Alive.Add(this);
    private void OnDisable() => Alive.Remove(this);

    // Called by the spawner right after Instantiate
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

        // Applying additional attributes

        if (definition.specialAttributes != null)
        {
            foreach (EnemyAttributeDefinition attribute in definition.specialAttributes)
            {
                if (attribute != null)
                {
                    attribute.ApplyTo(this);
                }
            }
        }
        Spawned?.Invoke(this);
    }

    private void Update()
    {
        if (!IsAlive || agent == null) return;
        if (!agent.isOnNavMesh) return;
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
            // On death, call listeners to trigger appropriate methods
            Died?.Invoke(this);
            GameController.Instance.EnemyKilled(this);
            Destroy(gameObject);
        }
    }

    public void ReachEnd()
    {
        if (!IsAlive) return;
        IsAlive = false;
        // Reach end invokes death too
        Died?.Invoke(this);
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

    // Remaining path distance to HQ (smaller --> closer)
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