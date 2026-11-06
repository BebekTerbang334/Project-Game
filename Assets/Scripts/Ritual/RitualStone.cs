using System;
using UnityEngine;

namespace Hutan.Ritual
{
    public sealed class RitualStone : MonoBehaviour, IInteractable
    {
        [SerializeField] private GameObject _placedVisual;
        public bool Filled { get; private set; }
        private Action<RitualStone> _onPlaced;
        public void Initialize(Action<RitualStone> onPlaced)
        {
            Filled = false;
            _onPlaced = onPlaced;
            if (_placedVisual != null) _placedVisual.SetActive(false);
        }
        public void ConfirmPhysicalPlacement()
        {
            if (Filled) return;
            Filled = true;
            _onPlaced?.Invoke(this);
        }
        public string GetPrompt(IncenseInventory inventory)
        {
            if (Filled) return "Batu sudah berisi dupa";
            return inventory.HasIncense ? "[E] Taruh dupa" : "Batu ritual — perlu satu dupa";
        }
        public string Interact(IncenseInventory inventory)
        {
            if (Filled) return "Dupa sudah terpasang di batu ini";
            if (!inventory.HasIncense) return "Ambil dupa dahulu";
            Filled = true;
            if (!inventory.TryPlaceIncense()) { Filled = false; return "Tidak ada dupa"; }
            if (_placedVisual != null) _placedVisual.SetActive(true);
            _onPlaced?.Invoke(this);
            return "Dupa ditaruh pada batu ritual";
        }
    }
}
