using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class GhostRoaming : MonoBehaviour
{
    [Header("Roaming Area")]
    public Transform[] waypoints;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float minWaitTime = 1f;
    public float maxWaitTime = 3f;

    private NavMeshAgent agent;
    private int previousWaypoint = -1;
    private bool isWaiting = false;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (agent == null)
        {
            Debug.LogError("Ghost membutuhkan NavMeshAgent!", this);
            enabled = false;
            return;
        }

        agent.speed = moveSpeed;
        agent.stoppingDistance = 0.3f;
    }

    void Start()
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogWarning("Tambahkan waypoint untuk Ghost!", this);
            return;
        }

        ChooseNextWaypoint();
    }

    void Update()
    {
        if (!agent.isOnNavMesh)
            return;

        if (waypoints == null || waypoints.Length == 0)
            return;

        if (isWaiting || agent.pathPending)
            return;

        if (agent.hasPath && agent.remainingDistance <=
            agent.stoppingDistance + 0.1f)
        {
            StartCoroutine(WaitAndMove());
        }
        else if (!agent.hasPath)
        {
            ChooseNextWaypoint();
        }
    }

    IEnumerator WaitAndMove()
    {
        isWaiting = true;
        agent.isStopped = true;

        yield return new WaitForSeconds(
            Random.Range(minWaitTime, maxWaitTime)
        );

        agent.isStopped = false;
        isWaiting = false;

        ChooseNextWaypoint();
    }

    void ChooseNextWaypoint()
    {
        if (!agent.isOnNavMesh)
            return;

        if (waypoints == null || waypoints.Length == 0)
            return;

        if (waypoints.Length == 1)
        {
            agent.SetDestination(waypoints[0].position);
            previousWaypoint = 0;
            return;
        }

        int nextWaypoint;

        do
        {
            nextWaypoint = Random.Range(0, waypoints.Length);
        }
        while (nextWaypoint == previousWaypoint);

        previousWaypoint = nextWaypoint;

        agent.SetDestination(waypoints[nextWaypoint].position);
    }
}