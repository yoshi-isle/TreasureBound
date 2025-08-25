using System;
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
    private bool groundedPlayer;
    private Vector3 playerVelocity;
    public Collectable currentCollectableFocused;
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
        print(GameManager.Instance.CurrentSaveData.CurrentXP);
        switch (PauseManager.Instance.gameState)
        {
            case PauseManager.GameState.Normal:
                HandleMovement();
                HandleMouseLook();
                Scan();
                break;
            case PauseManager.GameState.Paused:
                return;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    void Scan()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, 20f))
        {
            Collectable Collectable = hit.collider.GetComponent<Collectable>();
            if (Collectable != null)
            {
                currentCollectableFocused = Collectable;
                GameManager.Instance?.TriggerCollectableFocused(Collectable, hit.point);
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
    }
 
    void HandleMovement()
    {
        groundedPlayer = characterController.isGrounded;
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");
        Vector3 inputDir = new Vector3(moveX, 0, moveZ).normalized;
        Vector3 move = transform.TransformDirection(inputDir);

        bool wantsToSprint = Input.GetKey(KeyCode.LeftShift) && inputDir.magnitude > 0.1f && stamina > 0.1f;
        isSprinting = wantsToSprint && groundedPlayer;
        float targetSpeed = isSprinting ? moveSpeed * 1.7f : moveSpeed;

        if (groundedPlayer && playerVelocity.y < 0)
            playerVelocity.y = -2f;

        if (Input.GetKeyDown(KeyCode.Space) && groundedPlayer)
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y);

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

        float fovTarget = isSprinting ? 80f : 60f;
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
