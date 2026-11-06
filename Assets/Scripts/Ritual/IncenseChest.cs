using UnityEngine;

namespace Hutan.Ritual
{
    public sealed class IncenseChest : MonoBehaviour, IInteractable
    {
        [SerializeField] private int _initialStock = 4;
        [SerializeField] private Transform _lid;
        [SerializeField] private GameObject[] _stockVisuals;
        [SerializeField] private PickupItem _pickupPrefab;
        [SerializeField] private PickupItem[] _storedItems;
        public int Stock { get; private set; }
        private Quaternion _closedRotation;
        private bool _cached;
        public bool TryTakeIncense(PlayerInteract player)
        {
            if (Stock <= 0 || player == null || player.IsHolding) return false;
            bool authored = _storedItems != null && _storedItems.Length == _initialStock;
            if (!authored && _pickupPrefab == null) return false;
            var pickup = authored ? _storedItems[Stock - 1] : Instantiate(_pickupPrefab, transform.position, Quaternion.identity, transform.parent);
            if (pickup == null) return false;
            if (!player.TryHold(pickup)) { if (!authored) Destroy(pickup.gameObject); return false; }
            Stock--;
            if (_lid != null) _lid.localRotation = _closedRotation * Quaternion.Euler(65f, 0f, 0f);
            RefreshVisuals();
            return true;
        }
        private void Awake() { CacheLid(); ResetSession(); }
        private void CacheLid()
        {
            if (_cached) return;
            if (_lid != null) _closedRotation = _lid.localRotation;
            _cached = true;
        }
        public void ResetSession()
        {
            CacheLid();
            Stock = _initialStock;
            if (_lid != null) _lid.localRotation = _closedRotation;
            if (_storedItems != null)
                foreach (var item in _storedItems) if (item != null) item.PrepareStored();
            RefreshVisuals();
        }
        public string GetPrompt(IncenseInventory inventory)
        {
            if (Stock == 0) return "Peti kosong";
            return inventory.HasIncense ? "Tangan penuh — taruh dupa dahulu" : "[E] Ambil dupa dari peti (" + Stock + " tersisa)";
        }
        public string Interact(IncenseInventory inventory)
        {
            if (Stock <= 0) return "Peti kosong";
            if (!inventory.TryTakeIncense()) return "Hanya bisa membawa satu dupa";
            Stock--;
            if (_lid != null) _lid.localRotation = _closedRotation * Quaternion.Euler(65f, 0f, 0f);
            RefreshVisuals();
            return "Dupa diambil dari peti — " + Stock + " tersisa";
        }
        private void RefreshVisuals()
        {
            if (_stockVisuals == null) return;
            for (int i = 0; i < _stockVisuals.Length; i++)
                if (_stockVisuals[i] != null) _stockVisuals[i].SetActive(i < Stock);
        }
    }
}
