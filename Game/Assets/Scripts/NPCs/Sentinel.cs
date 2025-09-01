using System;
using System.Collections;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Sentinel : MonoBehaviour
{
    public Light pointLight;
    public Transform target;
    public Interactable interactable;
    public Transform[] patrolPoints;
    NavMeshAgent agent;
    private int currentPatrolIndex = 0;
    private float susTimer = 0f;
    private float stateChangeCooldown = 0f;
    public enum States
    {
        Initializing,
        Patrolling,
        Chasing,
        Suspicious
    }
    public States currentState = States.Patrolling;


    void OnTriggerExit(Collider other)
    {
        if (currentState != States.Chasing) return;


        if (other.CompareTag("Player"))
        {
            stateChangeCooldown = 1f;
            currentState = States.Suspicious;
            target = null;
        }
    }
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        Invoke("InitializePatrol", 1.0f);
        print("Starting scan coroutine");
        StartCoroutine(ScanForPlayer());
    }

    private IEnumerator ScanForPlayer()
    {
        print("Scanning for player...");
        while (true)
        {
            yield return new WaitForSeconds(0.2f);
            if (currentState == States.Patrolling)
            {
                //raycast from the front of the npc (me) to player.
                //check if there's any blockers in the way
                RaycastHit hit;
                Vector3 playerPosition = FindAnyObjectByType<FirstPersonController>().transform.position;
                Vector3 direction = (playerPosition - transform.position).normalized;
                if (Physics.Raycast(transform.position, direction, out hit, 8f))
                {
                    if (hit.collider.CompareTag("Player"))
                    {
                        print("Found.");
                        pointLight.color = Color.red;
                        currentState = States.Chasing;
                        target = hit.collider.transform;
                    }
                }
            }
        }
    }

    void OnEnable()
    {
        currentState = States.Patrolling;
    }

    void Update()
    {
        switch (currentState)
        {
            case States.Initializing:
                break;
            case States.Patrolling:
                agent.speed = 3;
                if (ReachedDestination())
                {
                    PatrolNextPoint();
                }
                stateChangeCooldown -= Time.deltaTime;
                pointLight.color = Color.blue;
                break;
            case States.Chasing:
                agent.speed = 5;
                agent.SetDestination(target.position);
                break;
            case States.Suspicious:
                pointLight.color = Color.yellow;
                susTimer += Time.deltaTime;
                if (susTimer >= 4f)
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
        if (agent.pathPending) return false;
        return agent.remainingDistance <= agent.stoppingDistance;
    }
}