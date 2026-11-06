using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PickupItem : MonoBehaviour
{
    public string itemID = "Dupa";

    private Rigidbody rb;
    private Collider[] colliders;
    private Light[] pickupLights;
    private Vector3 originalScale;
    private bool cached;
    [SerializeField] private bool _sceneAuthored;
    public bool SceneAuthored => _sceneAuthored;
    public bool IsPlaced { get; private set; }
    public bool IsHeld { get; private set; }

    void Awake() { CacheComponents(); }
    private void CacheComponents()
    {
        if (cached) return;
        rb = GetComponent<Rigidbody>();
        colliders = GetComponentsInChildren<Collider>(true);
        pickupLights = GetComponentsInChildren<Light>(true);
        originalScale = transform.localScale;
        cached = true;
    }
    public void ResetAt(Transform home)
    {
        CacheComponents();
        home.gameObject.SetActive(true);
        transform.SetParent(home, false);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = originalScale;
        rb.isKinematic = true;
        IsHeld = false;
        IsPlaced = false;
        gameObject.SetActive(true);
        SetColliders(true);
        foreach (var light in pickupLights) if (light != null) light.enabled = true;
    }
    public void PrepareStored()
    {
        CacheComponents();
        SetColliders(false);
        foreach (var light in pickupLights) if (light != null) light.enabled = false;
    }

    public void PickUp(Transform holdPoint)
    {
        if (holdPoint == null || IsPlaced || IsHeld) return;
        rb.isKinematic = true;
        SetColliders(false);
        foreach (var light in pickupLights) light.enabled = false;
        transform.SetParent(holdPoint, false);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = originalScale;
        IsHeld = true;
    }

    public void PlaceAt(Transform snapPoint)
    {
        if (snapPoint == null || !IsHeld || IsPlaced) return;
        // Keep the physical incense under its ritual socket for session cleanup.
        transform.SetParent(snapPoint, false);
        transform.localPosition = Vector3.zero;
        transform.rotation = Quaternion.identity;
        transform.localScale = originalScale;
        IsHeld = false;
        IsPlaced = true;
        // Placed incense cannot be picked up again.
    }

    private void SetColliders(bool state)
    {
        foreach (Collider c in colliders)
            if (c != null) c.enabled = state;
    }
}
