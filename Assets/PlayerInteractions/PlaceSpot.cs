using UnityEngine;
using UnityEngine.Events;

public class PlaceSpot : MonoBehaviour
{
    public Transform snapPoint;
    public string requiredItemID = "Dupa";
    public UnityEvent onPlaced;

    public bool IsOccupied { get; private set; }

    public void ResetSession() { IsOccupied = false; }

    public bool TryPlace(PickupItem item)
    {
        if (item == null || !item.IsHeld || IsOccupied || item.itemID != requiredItemID)
            return false;

        item.PlaceAt(snapPoint != null ? snapPoint : transform);
        if (!item.IsPlaced) return false;
        IsOccupied = true;
        onPlaced?.Invoke();
        return true;
    }
}