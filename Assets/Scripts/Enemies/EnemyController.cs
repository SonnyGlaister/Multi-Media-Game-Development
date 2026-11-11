using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.UI;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    public Image RemainingHealth;
    public Renderer bodyRenderer;

    public EnemyDefinition Definition { get; private set; }
    public FactionDefinition Faction => Definition != null ? Definition.faction : null;
    public float RemainingDistance;
    public bool IsAlive { get; private set; } = true;

    private int health;
    private float speed;
    private UnityEngine.AI.NavMeshAgent agent;
    private Vector3 startPosition;


    // Called by the spawner right after Instantiate
    public void Init(EnemyDefinition definition)
    {
        Definition = definition;
        name = definition.displayName;
        health = definition.maxHealth;
        transform.localScale = Vector3.one * definition.scale;
        speed = definition.speed;

        agent = GetComponent<NavMeshAgent>();
        agent.speed = speed;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;

        if (definition.modelPrefab != null)
        {
            bodyRenderer.enabled = false;
            Instantiate(definition.modelPrefab, transform);
        }
        else if (bodyRenderer != null)
        {
            bodyRenderer.material.color = definition.Tint;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        startPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsAlive || agent == null) return;

        if (!agent.isOnNavMesh) return;

        if (agent.pathPending || !agent.hasPath) return;

        RemainingDistance = agent.remainingDistance;

        // Stopping Distance is how far the agent needs to be before it counts as arrived, currently set to 0.5
        if (RemainingDistance <= agent.stoppingDistance)
        {
            ReachEnd();
        }
    }

    public void TakeDamage(int amount)
    {
        if (!IsAlive) return;

        health -= amount;
        if (health <= 0)
        {
            IsAlive = false;
            GameController.Instance.EnemyKilled(Definition.reward);
            Destroy(gameObject);
        }

        if (RemainingHealth != null && Definition.maxHealth > 0)
        {
            RemainingHealth.fillAmount = Mathf.Clamp01((float)health / Definition.maxHealth);
        }
    }

    public void ReachEnd()
    {
        IsAlive = false;
        GameController.Instance.EnemyReachedBase(Definition);
        Destroy(gameObject);
    }

    public void SetTargetDestination (Vector3 target)
    {
        agent.SetDestination(target);
        if (!agent.isOnNavMesh)
        {
            Debug.LogError("Enemy is not on the NavMesh");
            return;
        }
    }
    // LET THE RECORD SHOW I HATE THIS GODDAMN FUNCTION AND YEARN FOR A BETTER WAY TO TRACK ENEMY PROGRESS
    public float GetDistanceAlongPath() 
    {
        Vector3[] corners = agent.path.corners;

        if (corners.Length < 2)
            return 0f;

        Vector3 position = agent.nextPosition;
        float distance = 0f;

        for (int i = 0; i < corners.Length - 1; i++)
        {
            Vector3 a = corners[i];
            Vector3 b = corners[i + 1];

            Vector3 segment = b - a;

            float t = Mathf.Clamp01(
                Vector3.Dot(position - a, segment) / segment.sqrMagnitude
            );

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
