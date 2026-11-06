using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    public Camera playerCamera;
    public Transform holdPoint;
    public float interactRange = 3f;
    public KeyCode interactKey = KeyCode.E;
    [SerializeField] private Hutan.Ritual.IncenseInventory _inventory;

    private PickupItem heldItem;
    private System.IDisposable _keySubscription;
    private bool _interactionRequested;
    private readonly RaycastHit[] _hits = new RaycastHit[64];
    public PickupItem HeldItem => heldItem;
    public bool IsHolding => heldItem != null;

    void OnEnable() { SubscribeInput(); }
    void OnDisable()
    {
        _keySubscription?.Dispose();
        _keySubscription = null;
        _interactionRequested = false;
    }
    void SubscribeInput()
    {
        if (_keySubscription != null) return;
        _keySubscription = UnityEngine.InputSystem.Utilities.Observable.Call(
            UnityEngine.InputSystem.InputSystem.onAnyButtonPress, control =>
            {
                var keyboard = control.device as UnityEngine.InputSystem.Keyboard;
                if (keyboard != null && control == keyboard.eKey) _interactionRequested = true;
            });
    }
    public void ResetSession()
    {
        if (heldItem != null && !heldItem.SceneAuthored)
        {
            heldItem.gameObject.SetActive(false);
            Destroy(heldItem.gameObject);
        }
        heldItem = null;
        _interactionRequested = false;
        _keySubscription?.Dispose();
        _keySubscription = null;
        if (isActiveAndEnabled) SubscribeInput();
    }
    void LateUpdate()
    {
        if (!_interactionRequested) return;
        _interactionRequested = false;
        InteractNow();
    }
    public bool InteractNow()
    {
        if (playerCamera == null || holdPoint == null || _inventory == null ||
            !Application.isFocused || Cursor.lockState != CursorLockMode.Locked) return false;
        return heldItem == null ? TryPickUp() : TryPlace();
    }
    public bool TryHold(PickupItem item)
    {
        if (heldItem != null || holdPoint == null || _inventory == null || item == null ||
            item.IsHeld || item.IsPlaced || item.itemID != "Dupa") return false;
        if (!_inventory.TryTakeIncense()) return false;
        item.PickUp(holdPoint);
        if (!item.IsHeld) { _inventory.TryPlaceIncense(); return false; }
        heldItem = item;
        return true;
    }
    bool TryPickUp()
    {
        var collider = TargetCollider();
        if (collider == null) return false;
        var item = collider.GetComponentInParent<PickupItem>();
        if (item != null) return TryHold(item);
        var chest = collider.GetComponentInParent<Hutan.Ritual.IncenseChest>();
        if (chest != null) return chest.TryTakeIncense(this);
        var tirta = collider.GetComponentInParent<Hutan.Ritual.TirtaPickup>();
        if (tirta == null) return false;
        tirta.Interact(_inventory);
        return tirta.Collected;
    }
    bool TryPlace()
    {
        var collider = TargetCollider();
        if (collider == null || !_inventory.HasIncense) return false;
        var spot = collider.GetComponentInParent<PlaceSpot>();
        if (spot == null || !spot.TryPlace(heldItem)) return false;
        heldItem = null;
        _inventory.TryPlaceIncense();
        return true;
    }
    Collider TargetCollider()
    {
        int count = Physics.RaycastNonAlloc(playerCamera.transform.position, playerCamera.transform.forward,
            _hits, interactRange, ~0, QueryTriggerInteraction.Ignore);
        if (count == _hits.Length) return null;
        Collider nearest = null;
        float distance = float.PositiveInfinity;
        for (int i = 0; i < count; i++)
        {
            if (_hits[i].collider.transform.IsChildOf(transform) || _hits[i].distance >= distance) continue;
            nearest = _hits[i].collider;
            distance = _hits[i].distance;
        }
        return nearest;
    }
}
