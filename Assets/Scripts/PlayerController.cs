using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 3.5f;
    public float sprintSpeed = 5.5f;
    public float gravity = -20f;

    [Header("Mouse Look")]
    public Transform cameraTransform;
    public float mouseSensitivity = 150f;
    public float maxLookAngle = 85f;

    [Header("Head Bob")]
    public bool enableHeadBob = true;
    public float bobFrequency = 7f;
    public float bobAmplitude = 0.035f;

    private CharacterController controller;

    private float verticalVelocity;
    private float cameraPitch;

    private Vector3 cameraStartPosition;
    private float bobTimer;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        cameraStartPosition = cameraTransform.localPosition;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        Look();
        Move();
        HeadBob();

        // Tekan Escape untuk melepas mouse
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // Klik lagi untuk mengunci mouse
        if (Input.GetMouseButtonDown(0))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void Look()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Putar badan kiri-kanan
        transform.Rotate(Vector3.up * mouseX);

        // Putar kamera atas-bawah
        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -maxLookAngle, maxLookAngle);

        cameraTransform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
    }

    void Move()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 direction =
            transform.right * horizontal +
            transform.forward * vertical;

        direction = Vector3.ClampMagnitude(direction, 1f);

        float currentSpeed =
            Input.GetKey(KeyCode.LeftShift)
            ? sprintSpeed
            : walkSpeed;

        Vector3 movement = direction * currentSpeed;

        // Gravity
        if (controller.isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;
        }

        verticalVelocity += gravity * Time.deltaTime;

        movement.y = verticalVelocity;

        controller.Move(movement * Time.deltaTime);
    }

    void HeadBob()
    {
        if (!enableHeadBob)
            return;

        Vector3 horizontalVelocity = controller.velocity;
        horizontalVelocity.y = 0;

        if (horizontalVelocity.magnitude > 0.1f && controller.isGrounded)
        {
            bobTimer += Time.deltaTime * bobFrequency;

            float bobY = Mathf.Sin(bobTimer) * bobAmplitude;
            float bobX = Mathf.Cos(bobTimer * 0.5f) * bobAmplitude * 0.5f;

            cameraTransform.localPosition =
                cameraStartPosition +
                new Vector3(bobX, bobY, 0f);
        }
        else
        {
            bobTimer = 0;

            cameraTransform.localPosition =
                Vector3.Lerp(
                    cameraTransform.localPosition,
                    cameraStartPosition,
                    Time.deltaTime * 8f
                );
        }
    }
}