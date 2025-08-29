using System;
using UnityEngine;

public class NpcPatrol : MonoBehaviour
{
    public Transform[] patrolPoints;
    public float speed = 3f;
    public float reachDistance = 0.1f;
    public BoxCollider aggroRange;
    private Transform playerTransform;
    private CharacterController characterController;

    private int currentPoint = 0;
    public enum PatrolState
    {
        Patrolling,
        Suspicious,
        PlayerFound
    }
    public PatrolState State
    {
        get { return state; }
        set { state = value; print("Adjusting location"); AdjustTargetLocation(); }
    }

    [SerializeField] private PatrolState state = PatrolState.Patrolling;

    void Start()
    {
        AdjustTargetLocation();
        aggroRange = GetComponent<BoxCollider>();
        characterController = GetComponent<CharacterController>();
        
        // Add CharacterController if it doesn't exist
        if (characterController == null)
        {
            characterController = gameObject.AddComponent<CharacterController>();
            characterController.radius = 0.5f;
            characterController.height = 2f;
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
        switch (state)
        {
            case PatrolState.Patrolling:
                HandlePatrol();
                break;
            case PatrolState.Suspicious:
                break;
            case PatrolState.PlayerFound:
                HandlePlayerFound();
                break;
        }

    }

    private void HandlePlayerFound()
    {
        Transform target = playerTransform;

        transform.position = Vector3.MoveTowards(
            new Vector3(transform.position.x, 0, transform.position.z),
            new Vector3(target.position.x, 0, target.position.z),
            speed * Time.deltaTime
        );
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

        Transform target = patrolPoints[currentPoint];

        transform.position = Vector3.MoveTowards(
            new Vector3(transform.position.x, 0, transform.position.z),
            new Vector3(target.position.x, 0, target.position.z),
            speed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, target.position) < reachDistance)
        {
            currentPoint++;
            if (currentPoint >= patrolPoints.Length)
            {
                currentPoint = 0;
            }
        }
    }
    
    private void AdjustTargetLocation()
    {
        RaycastHit hit;
        Vector3 start = transform.position + Vector3.up * 0.5f;
        Vector3 end = patrolPoints[currentPoint].position + Vector3.up * 0.5f;
        Vector3 direction = (end - start).normalized;
        float distance = Vector3.Distance(start, end);

        if (Physics.Raycast(start, direction, out hit, distance))
        {
            currentPoint++;
            if (currentPoint >= patrolPoints.Length)
            {
                currentPoint = 0;
            }
            return;
        }
    }
}
