using System;
using System.Linq;
using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 80f;
    public float jumpHeight = 1.5f;
    private CharacterController characterController;
    private Transform cameraTransform;
    private float verticalLookRotation = 0f;
    public float hitpoints = 100f;
    public float stamina = 100f;
    public Vector3 cameraOffset = new Vector3(0f, 1.6f, 0f);
    private bool isSprinting = false;
    private bool staminaRecharging = false;
    private float staminaRechargeTimer = 2f;
    private Vector3 playerVelocity;
    public Interactable currentInteractableFocused;
    private Inventory inventory;
    public enum PlayerState
    {
        Normal,
        Inactive
    }
    public PlayerState playerState = PlayerState.Normal;


    void Awake()
    {
        Time.timeScale = 1.0f;
        inventory = FindAnyObjectByType<Inventory>();
    }
    void Start()
    {
        GameManager.Instance.OnGameRestart += () => playerState = PlayerState.Normal;
        GameManager.Instance.OnPlayerDead += () => playerState = PlayerState.Inactive;

        characterController = GetComponent<CharacterController>();
        cameraTransform = Camera.main.transform;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        switch (PauseManager.Instance.gameState)
        {
            case PauseManager.GameState.Normal:
                playerState = PlayerState.Normal;
                HandleMovement();
                Scan();
                ListenForActionInputs();
                break;
            case PauseManager.GameState.Paused:
                playerState = PlayerState.Inactive;
                return;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void ListenForActionInputs()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (currentInteractableFocused != null)
            {
                GameManager.Instance?.TriggerInteractableUnfocused();
                currentInteractableFocused.Interact();
                currentInteractableFocused = null;
            }
        }
    }

    void Scan()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 10f))
        {
            Interactable interactable = hit.collider.GetComponent<Interactable>();
            if (interactable != null)
            {
                currentInteractableFocused = interactable;
                GameManager.Instance?.TriggerInteractableFocused(interactable, hit.point);
            }
            else
            {
                // If we had a collectable before, trigger the unfocused event
                if (currentInteractableFocused != null)
                {
                    GameManager.Instance?.TriggerInteractableUnfocused();
                    currentInteractableFocused = null;
                }
            }
        }
        else
        {
            if (currentInteractableFocused != null)
            {
                GameManager.Instance?.TriggerInteractableUnfocused();
                currentInteractableFocused = null;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Hitbox"))
        {
            print("im hit");
            Time.timeScale = 0f;
            GameManager.Instance.TriggerOnPlayerDead();
        }
    }

    void HandleMovement()
    {
        Vector3 inputDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        Vector3 move = transform.TransformDirection(inputDir);

        bool movingForward = inputDir.z > 0.1f && Mathf.Abs(inputDir.x) < 0.1f;
        bool wantsToSprint = Input.GetKey(KeyCode.LeftShift) && movingForward && stamina > 0.1f;
        if (characterController.isGrounded)
        {
            isSprinting = wantsToSprint;
        }
        float targetSpeed = isSprinting ? moveSpeed * 1.7f : moveSpeed;

        if (characterController.isGrounded && playerVelocity.y < 0)
            playerVelocity.y = -2f;

        if (Input.GetKeyDown(KeyCode.Space) && characterController.isGrounded)
        {
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y);
        }

        playerVelocity.y += Physics.gravity.y * Time.deltaTime;

        if (isSprinting)
        {
            stamina -= 25f * Time.deltaTime;
            if (stamina < 0f) stamina = 0f;
            staminaRechargeTimer = 1.2f;
            staminaRecharging = false;
        }
        else
        {
            if (stamina < 100f)
            {
                if (!staminaRecharging)
                {
                    staminaRechargeTimer -= Time.deltaTime;
                    if (staminaRechargeTimer <= 0f)
                        staminaRecharging = true;
                }
                if (staminaRecharging)
                {
                    stamina += 18f * Time.deltaTime;
                    if (stamina > 100f) stamina = 100f;
                }
            }
            else
            {
                staminaRechargeTimer = 1.2f;
                staminaRecharging = false;
            }
        }

        Vector3 velocity = move * targetSpeed;
        // If airborne, reduce movement speed unless sprint-jumping forward
        if (!characterController.isGrounded)
        {
            if (!(isSprinting && inputDir.z > 0.1f && Mathf.Abs(inputDir.x) < 0.1f))
            {
            velocity *= 0.6f;
            }
        }
        characterController.Move((velocity + new Vector3(0, playerVelocity.y, 0)) * Time.deltaTime);
    }

    void LateUpdate()
    {
        switch (playerState)
        {
            case PlayerState.Normal:
                HandleMouseLook();
                cameraTransform.position = transform.position + transform.TransformVector(cameraOffset);
                break;
            case PlayerState.Inactive:
                break;
            default:
                break;
        }
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        verticalLookRotation -= mouseY;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -maxLookAngle, maxLookAngle);
        transform.Rotate(Vector3.up * mouseX);
        cameraTransform.rotation = Quaternion.Euler(verticalLookRotation, transform.eulerAngles.y, 0f);
    }
}
