using UnityEngine;

namespace Hutan.Ritual
{
    public sealed class IncensePickup : MonoBehaviour, IInteractable
    {
        public bool Collected { get; private set; }
        public string GetPrompt(IncenseInventory inventory)
        {
            if (Collected) return string.Empty;
            return inventory.HasIncense ? "Tangan penuh — taruh dupa dahulu" : "[E] Ambil dupa";
        }
        public string Interact(IncenseInventory inventory)
        {
            if (Collected) return "Dupa sudah diambil";
            if (!inventory.TryTakeIncense()) return "Hanya bisa membawa satu dupa";
            Collected = true;
            gameObject.SetActive(false);
            return "Dupa diambil — bawa ke batu ritual";
        }
    }
}
