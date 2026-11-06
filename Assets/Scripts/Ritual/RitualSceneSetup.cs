using UnityEngine;
using UnityEngine.SceneManagement;

namespace Hutan.Ritual
{
    public sealed class RitualSceneSetup : MonoBehaviour
    {
        [SerializeField] private IncenseInventory _inventory;
        [SerializeField] private PlayerInteract _interactor;
        [SerializeField] private RitualManager _manager;
        [SerializeField] private PickupItem[] _incense;
        [SerializeField] private Transform[] _homes;
        [SerializeField] private IncenseChest _chest;
        [SerializeField] private RitualStone[] _stones;
        [SerializeField] private Light _centerLight;
        [SerializeField] private Transform _tirtaPoint;
        [SerializeField] private Transform _rewardRoot;
        public bool Ready { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void ResetLoadedScenes()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
                foreach (var root in SceneManager.GetSceneAt(i).GetRootGameObjects())
                    foreach (var setup in root.GetComponentsInChildren<RitualSceneSetup>(true))
                        if (setup.isActiveAndEnabled) setup.BeginSession();
        }
        private void Start() { if (!Ready) BeginSession(); }
        public void BeginSession()
        {
            Ready = false;
            if (_inventory == null || _interactor == null || _manager == null || _chest == null ||
                _centerLight == null || _tirtaPoint == null || _rewardRoot == null ||
                _incense == null || _homes == null || _incense.Length != 7 || _homes.Length != 7 ||
                _stones == null || _stones.Length != 7)
            {
                Debug.LogError("Referensi objek ritual di Hierarchy belum lengkap", this);
                return;
            }
            for (int i = 0; i < 7; i++)
                if (_incense[i] == null || _homes[i] == null || _stones[i] == null)
                {
                    Debug.LogError("Dupa, posisi awal atau batu ritual hilang", this);
                    return;
                }
            _interactor.ResetSession();
            _inventory.ResetSession();
            foreach (Transform child in _rewardRoot)
            {
                child.gameObject.SetActive(false);
                Destroy(child.gameObject);
            }
            for (int i = 0; i < 7; i++) _incense[i].ResetAt(_homes[i]);
            _chest.ResetSession();
            foreach (var stone in _stones)
            {
                var spot = stone.GetComponent<PlaceSpot>();
                if (spot != null) spot.ResetSession();
            }
            _manager.Initialize(_stones, _centerLight, _rewardRoot, _tirtaPoint.position);
            Physics.SyncTransforms();
            Ready = true;
        }
    }
}
