using UnityEngine;
using System.Collections.Generic;
using System;

public class GuardianSimplePatrol : MonoBehaviour
{
    List<Transform> patrolPoints;
    Transform currentPatrolPointTarget;
    CharacterController characterController;

    PatrolStates currentState = PatrolStates.Patrolling;

    enum PatrolStates
    {
        Patrolling,
        PlayerSpotted,
        Suspicious
    }
    private int currentIndex = 0;
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        patrolPoints = new List<Transform>();
        Transform parentRoom = transform.parent.parent;
        if (parentRoom != null)
        {
            foreach (Transform child in parentRoom)
            {
                if (child.name.Contains("Patrol"))
                {
                    patrolPoints.Add(child);
                }
            }
            if (patrolPoints.Count > 0)
            {
                currentIndex = 0;
                currentPatrolPointTarget = patrolPoints[0];
            }
        }
    }

    void Update()
    {
        switch (currentState)
        {
            case PatrolStates.Patrolling:
                HandlePatrolUpdate();
                break;
            case PatrolStates.PlayerSpotted:
                HandlePlayerSpotted();
                break;
            case PatrolStates.Suspicious:
                HandleSuspicious();
                break;
        }

    }

    private void HandleSuspicious()
    {
        print("Method not yet implemented.");
    }

    private void HandlePlayerSpotted()
    {
        print("Method not yet implemented.");
    }

    private void HandlePatrolUpdate()
    {
        if (currentPatrolPointTarget != null)
        {
            var manhattanDistance = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(currentPatrolPointTarget.position.x, 0, currentPatrolPointTarget.position.z));
            if (manhattanDistance < 1f)
            {
                currentIndex = (currentIndex + 1) % patrolPoints.Count;
                currentPatrolPointTarget = patrolPoints[currentIndex];
            }

            Vector3 direction = (currentPatrolPointTarget.position - transform.position).normalized;

            RaycastHit hit;
            if (Physics.Raycast(transform.position, direction, out hit, 1.5f))
            {
                if (hit.transform != currentPatrolPointTarget)
                {
                    return;
                }
            }

            characterController.Move(direction * Time.deltaTime);
        }
    }
}
