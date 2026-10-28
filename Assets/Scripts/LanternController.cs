using UnityEngine;

public class LanternController : MonoBehaviour
{
    [Header("Movement Sway")]
    public float positionSway = 0.015f;
    public float rotationSway = 3f;

    [Header("Smoothing")]
    public float smoothSpeed = 8f;

    [Header("Walking Bob")]
    public float bobAmount = 0.025f;
    public float bobSpeed = 5f;

    private Vector3 startPosition;
    private Quaternion startRotation;

    private float bobTimer;

    void Start()
    {
        startPosition = transform.localPosition;
        startRotation = transform.localRotation;
    }

    void Update()
    {
        MouseSway();
        WalkingBob();
    }

    void MouseSway()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // Gerakan kecil akibat mouse
        float targetX = -mouseX * positionSway;
        float targetY = -mouseY * positionSway;

        Vector3 targetPosition =
            startPosition +
            new Vector3(targetX, targetY, 0f);

        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            targetPosition,
            Time.deltaTime * smoothSpeed
        );

        // Rotasi lentera
        Quaternion targetRotation =
            startRotation *
            Quaternion.Euler(
                mouseY * rotationSway,
                mouseX * rotationSway,
                -mouseX * rotationSway
            );

        transform.localRotation = Quaternion.Slerp(
            transform.localRotation,
            targetRotation,
            Time.deltaTime * smoothSpeed
        );
    }

    void WalkingBob()
    {
        if (Input.GetAxisRaw("Horizontal") != 0 ||
            Input.GetAxisRaw("Vertical") != 0)
        {
            bobTimer += Time.deltaTime * bobSpeed;

            float bobX = Mathf.Sin(bobTimer) * bobAmount;
            float bobY = Mathf.Abs(Mathf.Cos(bobTimer)) * bobAmount;

            transform.localPosition +=
                new Vector3(bobX, bobY, 0f);
        }
    }
}