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
    private bool isSprinting = false;
    private bool staminaRecharging = false;
    private float staminaRechargeTimer = 2f;
    private Vector3 playerVelocity;
    public Collectable currentCollectableFocused;
    Inventory inventory;

    void Awake()
    {
        inventory = GetComponent<Inventory>();
    }
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        cameraTransform = transform.Find("Camera");
        if (cameraTransform == null)
        {
            cameraTransform = GetComponentInChildren<Camera>().transform;
        }
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        switch (PauseManager.Instance.gameState)
        {
            case PauseManager.GameState.Normal:
                HandleMovement();
                HandleMouseLook();
                Scan();
                ListenForActionInputs();
                break;
            case PauseManager.GameState.Paused:
                return;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void ListenForActionInputs()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (currentCollectableFocused != null)
            {
                GameManager.Instance?.TriggerCollectableUnfocused();
                currentCollectableFocused.Collect();
                GameManager.Instance?.TriggerCollectablePickedUp(currentCollectableFocused);
                currentCollectableFocused = null;
            }
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            print(GetComponent<Inventory>().Bag.First().Name);
        }
    }

    void Scan()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 8f))
        {
            Collectable collectable = hit.collider.GetComponent<Collectable>();
            if (collectable != null)
            {
                currentCollectableFocused = collectable;
                GameManager.Instance?.TriggerCollectableFocused(collectable, hit.point);
            }
            else
            {
                // If we had a collectable before, trigger the unfocused event
                if (currentCollectableFocused != null)
                {
                    GameManager.Instance?.TriggerCollectableUnfocused();
                    currentCollectableFocused = null;
                }
            }
        }
        else
        {
            if (currentCollectableFocused != null)
            {
                GameManager.Instance?.TriggerCollectableUnfocused();
                currentCollectableFocused = null;
            }
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

        float fovTarget = isSprinting ? 110f : 60f;
        Camera cam = cameraTransform.GetComponent<Camera>();
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, fovTarget, Time.deltaTime * 7f);

        Vector3 velocity = move * targetSpeed;
        characterController.Move((velocity + new Vector3(0, playerVelocity.y, 0)) * Time.deltaTime);
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        verticalLookRotation -= mouseY;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -maxLookAngle, maxLookAngle);
        cameraTransform.localEulerAngles = new Vector3(verticalLookRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }
}
