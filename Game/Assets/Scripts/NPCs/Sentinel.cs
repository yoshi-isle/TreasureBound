using System;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Sentinel : MonoBehaviour
{
    public Light pointLight;
    public Transform target;
    public Transform[] patrolPoints;
    NavMeshAgent agent;
    private int currentPatrolIndex = 0;
    private float susTimer = 0f;
    public enum States
    {
        Patrolling,
        Chasing,
        Suspicious
    }
    public States currentState = States.Patrolling;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            pointLight.color = Color.red;
            currentState = States.Chasing;
            target = other.transform;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            currentState = States.Patrolling;
            target = null;
        }
    }
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        Invoke("InitializePatrol", 1.0f);
    }

    void Update()
    {
        switch (currentState)
        {
            case States.Patrolling:
                if (ReachedDestination())
                {
                    PatrolNextPoint();
                }
                pointLight.color = Color.blue;
                break;
            case States.Chasing:
                agent.speed = 5;
                agent.SetDestination(target.position);
                break;
            case States.Suspicious:
                susTimer += Time.deltaTime;
                if (susTimer >= 10f)
                {
                    currentState = States.Patrolling;
                    InitializePatrol();
                    susTimer = 0f;
                }
                break;
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