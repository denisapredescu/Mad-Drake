using UnityEngine;
using UnityEngine.AI;

public class FollowTarget : MonoBehaviour
{
    public Transform target;  // The target game object to follow
    NavMeshAgent agent;      // The navmesh agent component

    void Start()
    {
        // Get the navmesh agent component
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        // Set the destination of the navmesh agent to the target's position
        agent.SetDestination(target.position);
    }
}
