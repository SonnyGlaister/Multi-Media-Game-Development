using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FlyingAttributeBehaviour : MonoBehaviour
{
    public float height = 1.0f;

    // agent controls enemy's next position
    private NavMeshAgent agent;
    public bool flying = false;

    private void Awake()
    {
        // Get NavMeshAgent from current component of which this attribute is attached to.
        agent = GetComponent<NavMeshAgent>();
    }
    private void OnEnable()
    {
        if (agent != null)
        {
            agent.updatePosition = false;
        }

    }

    private void OnDisable()
    {
        if (agent != null)
        {
            agent.updatePosition = true;
        }
    }

    private void Update()
    {
        Vector3 increase = new Vector3(0, height, 0);
        transform.position = (agent.nextPosition + increase);
    }
}
