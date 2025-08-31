using System;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class Sentinel : MonoBehaviour
{
    public Transform target;
    public Transform[] patrolPoints;
    NavMeshAgent agent;
    private int currentPatrolIndex = 0; // Track current patrol point
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        Invoke("InitializePatrol", 1.0f);

    }

    void Update()
    {
        if (ReachedDestination())
        {
            // Patrol to the next point
            PatrolNextPoint();
        }
    }

    private void PatrolNextPoint()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        agent.SetDestination(patrolPoints[currentPatrolIndex].position);
    }

    private void InitializePatrol()
    {
        agent.enabled = true;
        enabled = true;

        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            print($"Found {patrolPoints.Length} patrol points");
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                if (patrolPoints[i] == null)
                {
                    Debug.LogWarning($"Patrol point {i} is null!");
                    continue;
                }
                if (patrolPoints[i].position == Vector3.zero)
                {
                    Debug.LogWarning($"Patrol point {i} is at (0,0,0)!");
                }
            }
            // Set to first valid patrol point
            currentPatrolIndex = 0;
            var firstValid = patrolPoints.FirstOrDefault(p => p != null);
            if (firstValid != null)
            {
                agent.SetDestination(firstValid.position);
            }
            else
            {
                Debug.LogWarning("No valid patrol points to set as destination.");
            }
        }
        else
        {
            print("No patrol points found! Guardian will not patrol.");
        }
    }

    private bool ReachedDestination()
    {
        return agent.remainingDistance <= agent.stoppingDistance;
    }
}