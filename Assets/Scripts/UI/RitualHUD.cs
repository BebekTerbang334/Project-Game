using UnityEngine;

namespace Hutan.Ritual
{
    public sealed class RitualHUD : MonoBehaviour
    {
        [SerializeField] private IncenseInventory _inventory;
        [SerializeField] private RitualManager _manager;
        [SerializeField] private TMPro.TextMeshProUGUI _status;

        private void OnEnable()
        {
            if (_inventory != null) _inventory.Changed += Refresh;
            if (_manager != null) _manager.Changed += Refresh;
            Refresh();
        }
        private void OnDisable()
        {
            if (_inventory != null) _inventory.Changed -= Refresh;
            if (_manager != null) _manager.Changed -= Refresh;
        }
        public void Refresh()
        {
            if (_inventory == null || _manager == null || _status == null) return;
            _status.text = "RITUAL DUPA  " + _manager.PlacedCount + "/7\n" +
                (_inventory.HasTirta ? "Tirta diperoleh" : _inventory.HasIncense ? "Dibawa: 1 dupa" : "Dibawa: kosong");
        }
    }
}
