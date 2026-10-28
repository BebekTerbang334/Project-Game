
using UnityEngine;

public class GhostFloating : MonoBehaviour
{
    [Header("Floating")]
    public float floatHeight = 0.15f;
    public float floatSpeed = 1.2f;

    [Header("Sway")]
    public float swayAngle = 3f;
    public float swaySpeed = 0.8f;

    private Vector3 originalPosition;
    private Quaternion originalRotation;

    void Start()
    {
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;
    }

    void Update()
    {
        float floating = Mathf.Sin(Time.time * floatSpeed)
                         * floatHeight;

        transform.localPosition =
            originalPosition + Vector3.up * floating;

        float sway = Mathf.Sin(Time.time * swaySpeed)
                     * swayAngle;

        transform.localRotation =
            originalRotation * Quaternion.Euler(0f, 0f, sway);
    }
}
