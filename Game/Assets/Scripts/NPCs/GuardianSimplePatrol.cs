using System;
using UnityEngine;

public class NpcPatrol : MonoBehaviour
{
    public Transform[] patrolPoints;
    public float speed = 3f;
    public float reachDistance = 0.1f;
    public float waitTime = 1f; // Time to wait at each patrol point
    public BoxCollider aggroRange;
    private Transform playerTransform;
    private CharacterController characterController;
    private Vector3 moveDirection = Vector3.zero;
    private float susTimer = 0;
    private int currentPoint = 0;
    private float waitTimer = 0f;
    private bool isWaiting = false;
    public enum PatrolState
    {
        Patrolling,
        Suspicious,
        PlayerFound
    }
    public PatrolState State
    {
        get { return state; }
        set { state = value; isWaiting = false; print("Adjusting location"); AdjustTargetLocation(); }
    }

    [SerializeField] private PatrolState state = PatrolState.Patrolling;

    void Start()
    {
        // Disable the script until the room is properly positioned
        enabled = false;
        
        aggroRange = GetComponent<BoxCollider>();
        characterController = GetComponent<CharacterController>();
        
        Invoke("InitializePatrol", 1.0f);
    }

    private void InitializePatrol()
    {
        enabled = true;
        
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            print($"Found {patrolPoints.Length} patrol points");
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                if (patrolPoints[i] != null)
                {
                    print($"Patrol point {i}: {patrolPoints[i].position}");
                }
            }
            AdjustTargetLocation();
        }
        else
        {
            print("No patrol points found! Guardian will not patrol.");
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            State = PatrolState.PlayerFound;
        }
    }

    void Update()
    {
        if (characterController.isGrounded)
        {
            moveDirection.y = 0;
        }
        else
        {
            moveDirection.y += Physics.gravity.y * Time.deltaTime;
        }

        switch (state)
        {
            case PatrolState.Patrolling:
                HandlePatrol();
                break;
            case PatrolState.Suspicious:
                HandleSuspicious();
                break;
            case PatrolState.PlayerFound:
                HandlePlayerFound();
                break;
        }

        if (state == PatrolState.Patrolling && isWaiting)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0)
            {
                isWaiting = false;
                currentPoint = (currentPoint + 1) % patrolPoints.Length;
            }
        }

        characterController.Move(moveDirection * Time.deltaTime);
    }

    private void HandleSuspicious()
    {
        susTimer += Time.deltaTime;
        if (susTimer >= 5f)
        {
            State = PatrolState.Patrolling;
            susTimer = 0;
        }
        moveDirection.x = 0;
        moveDirection.z = 0;
    }

    private void OnPlayerDetected(GameObject player)
    {
        playerTransform = player.transform;
        print("Player detected on the parent level");
        State = PatrolState.PlayerFound;
    }

    private void OnPlayerLost(GameObject player)
    {
        if (playerTransform == player.transform)
        {
            playerTransform = null;
            print("Player lost on the parent level");
            State = PatrolState.Suspicious;
        }
    }

    private void HandlePatrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        if (isWaiting)
        {
            moveDirection.x = 0;
            moveDirection.z = 0;
            return;
        }

        Transform target = patrolPoints[currentPoint];

        Vector3 direction = target.position - transform.position;
        direction.y = 0;

        if (direction.magnitude > reachDistance)
        {
            moveDirection.x = direction.normalized.x * speed;
            moveDirection.z = direction.normalized.z * speed;
        }
        else
        {
            // Reached the point, start waiting
            isWaiting = true;
            waitTimer = waitTime;
            moveDirection.x = 0;
            moveDirection.z = 0;
        }
    }

    private void HandlePlayerFound()
    {
        transform.LookAt(playerTransform);
        if (playerTransform == null) return;

        Vector3 direction = playerTransform.position - transform.position;
        direction.y = 0;
        moveDirection.x = direction.normalized.x * speed;
        moveDirection.z = direction.normalized.z * speed;
    }
    
    private void AdjustTargetLocation()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        int startingPoint = currentPoint;
        int checkedPoints = 0;

        while (checkedPoints < patrolPoints.Length)
        {
            RaycastHit hit;
            Vector3 start = transform.position + Vector3.up * 0.5f;
            Vector3 end = patrolPoints[currentPoint].position + Vector3.up * 0.5f;
            Vector3 direction = (end - start).normalized;
            float distance = Vector3.Distance(start, end);

            // If there's no obstacle to this patrol point, use it
            if (!Physics.Raycast(start, direction, out hit, distance))
            {
                print($"Target adjusted to patrol point {currentPoint}");
                return;
            }

            // Try the next point
            currentPoint = (currentPoint + 1) % patrolPoints.Length;
            checkedPoints++;
        }

        // If we've checked all points and they all have obstacles, 
        // just stay at the starting point and let normal patrol handle it
        currentPoint = startingPoint;
        print("All patrol points have obstacles, staying at current target");
    }
}
