// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.AI;

// public class NavMeshTest : MonoBehaviour
// {
//     public GameObject target;
//     private UnityEngine.AI.NavMeshAgent agent;
//     // Start is called before the first frame update
//     void Start()
//     {
//         agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
//         agent.SetDestination(target.transform.position);
//     }

//     // Update is called once per frame
//     void Update()
//     {
//         Debug.Log(agent.remainingDistance);
//     }
// }
using UnityEngine;
using UnityEngine.AI;

public class NavMeshTest : MonoBehaviour
{
    public GameObject target;
    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        bool result = agent.SetDestination(target.transform.position);
        agent.CalculatePath(target.transform.position, agent.path);

        Debug.Log("Destination accepted: " + result);
    }

    void Update()
    {
        Debug.Log("Remaining Distance: " + agent.remainingDistance);
        Debug.Log("Path corners: " + agent.path.corners.Length);
        Debug.Log("Has Path: " + agent.hasPath);
        // Debug.Log("Path Pending: " + agent.pathPending);
        // Debug.Log("Path Status: " + agent.pathStatus);
        // Debug.Log("Velocity: " + agent.velocity.magnitude);
    }
}