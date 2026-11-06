using UnityEngine;

namespace Hutan.Ritual
{
    public sealed class TirtaPickup : MonoBehaviour, IInteractable
    {
        public bool Collected { get; private set; }
        public string GetPrompt(IncenseInventory inventory)
        {
            if (Collected) return string.Empty;
            return inventory.HasIncense ? "Taruh dupa dahulu" : "[E] Ambil tirta";
        }
        public string Interact(IncenseInventory inventory)
        {
            if (Collected || !inventory.TryTakeTirta()) return "Tirta tidak dapat diambil";
            Collected = true;
            gameObject.SetActive(false);
            return "Tirta diperoleh — ritual selesai";
        }
    }
}
