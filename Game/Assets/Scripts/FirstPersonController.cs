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
    public Collectible currentCollectibleFocused;
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
        HandleMovement();
        HandleMouseLook();
        Scan();
    }

    void Scan()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, 20f))
        {
            Collectible collectible = hit.collider.GetComponent<Collectible>();
            if (collectible != null)
            {
                currentCollectibleFocused = collectible;
                GameManager.Instance?.TriggerCollectibleFocused(collectible, hit.point);
            }
            else
            {
                if (currentCollectibleFocused != null)
                {
                    GameManager.Instance?.TriggerCollectibleUnfocused();
                    currentCollectibleFocused = null;
                }
            }
        }
    }

    private void ResetStaminaRechargeTimer()
    {
        staminaRecharging = false;
        staminaRechargeTimer = 2f;
    }

    void HandleMovement()
    {
        groundedPlayer = characterController.isGrounded;
        
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }

        if (Input.GetKeyDown(KeyCode.Space) && groundedPlayer)
        {
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -2.0f * Physics.gravity.y);
        }

        playerVelocity.y += Physics.gravity.y * Time.deltaTime;

        isSprinting = Input.GetKey(KeyCode.LeftShift) && stamina > 0;
        float currentSpeed = isSprinting ? moveSpeed * 2 : moveSpeed;

        if (isSprinting)
        {
            ResetStaminaRechargeTimer();
        }

        if (!isSprinting && stamina < 100f && !staminaRecharging)
        {
            staminaRechargeTimer -= Time.deltaTime;
        }

        if (staminaRechargeTimer <= 0)
        {
            staminaRecharging = true;
        }

        if (staminaRecharging)
        {
            stamina += 20f * Time.deltaTime;
            staminaRechargeTimer = 10f;
        }

        if (stamina >= 100f)
        {
            ResetStaminaRechargeTimer();
        }

        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        Vector3 move = transform.right * moveX + transform.forward * moveZ;

        if (move.magnitude > 1f)
        {
            cameraTransform.GetComponent<Camera>().fieldOfView = Mathf.Lerp(cameraTransform.GetComponent<Camera>().fieldOfView, isSprinting ? 85f : 60f, Time.deltaTime * 3f);
            stamina -= isSprinting ? 30f * Time.deltaTime : 0;
        }

        // Combine horizontal movement with vertical velocity
        Vector3 finalMovement = move * currentSpeed * Time.deltaTime + playerVelocity * Time.deltaTime;
        characterController.Move(finalMovement);
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
