using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 80f;

    private CharacterController characterController;
    private Transform cameraTransform;
    private float verticalLookRotation = 0f;
    public float hitpoints = 100f;
    public float stamina = 100f;
    private bool isSprinting = false;
    private bool staminaRecharging = false;
    private float staminaRechargeTimer = 10f;

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
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, 20f))
        {
        }
    }

    void HandleMovement()
    {
        isSprinting = Input.GetKey(KeyCode.LeftShift) && stamina > 0;
        float currentSpeed = isSprinting ? moveSpeed * 2 : moveSpeed;
        if (!isSprinting && stamina < 100f && !staminaRecharging)
        {
            staminaRechargeTimer -= Time.deltaTime;
        }

        if (staminaRechargeTimer <= 0)
        {
            staminaRecharging = true;
            stamina += 20f * Time.deltaTime;
            staminaRechargeTimer = 10f;
        }

        if (stamina >= 100f)
        {
            staminaRecharging = false;
        }

        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        Vector3 move = transform.right * moveX + transform.forward * moveZ;

        if (move.magnitude > 1f)
        {
            cameraTransform.GetComponent<Camera>().fieldOfView = Mathf.Lerp(cameraTransform.GetComponent<Camera>().fieldOfView, isSprinting ? 80f : 60f, Time.deltaTime * 5f);
            stamina -= isSprinting ? 30f * Time.deltaTime : 0;
        }

        characterController.Move(move * currentSpeed * Time.deltaTime);
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
