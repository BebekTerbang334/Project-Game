using System;
using System.Collections.Generic;
using UnityEngine;

namespace Hutan.Ritual
{
    public sealed class RitualManager : MonoBehaviour
    {
        [SerializeField] private GameObject _tirtaPrefab;
        [SerializeField] private float _lightIntensity = 3f;
        [SerializeField] private float _lightFadeSeconds = 1.5f;
        private readonly HashSet<RitualStone> _stones = new HashSet<RitualStone>();
        private readonly HashSet<RitualStone> _filled = new HashSet<RitualStone>();
        private Light _centerLight;
        private Transform _rewardRoot;
        private Vector3 _rewardPosition;
        public const int RequiredIncense = 7;
        public int PlacedCount => _filled.Count;
        public bool Completed { get; private set; }
        public GameObject Reward { get; private set; }
        public event Action Changed;
        public event Action CompletedRitual;

        public void Initialize(RitualStone[] stones, Light centerLight, Transform rewardRoot, Vector3 rewardPosition)
        {
            _stones.Clear();
            _filled.Clear();
            Completed = false;
            Reward = null;
            _centerLight = centerLight;
            _rewardRoot = rewardRoot;
            _rewardPosition = rewardPosition;
            if (_centerLight != null) _centerLight.intensity = 0f;
            foreach (var stone in stones)
            {
                if (stone == null || !_stones.Add(stone)) continue;
                stone.Initialize(OnPlaced);
            }
            if (_stones.Count != RequiredIncense) Debug.LogError("Ritual harus memiliki tepat tujuh batu unik", this);
            Changed?.Invoke();
        }

        private void OnPlaced(RitualStone stone)
        {
            if (Completed || !_stones.Contains(stone) || !stone.Filled || !_filled.Add(stone)) return;
            if (_filled.Count == RequiredIncense)
            {
                Completed = true;
                if (_tirtaPrefab != null) Reward = Instantiate(_tirtaPrefab, _rewardPosition, Quaternion.identity, _rewardRoot);
                CompletedRitual?.Invoke();
            }
            Changed?.Invoke();
        }
        private void Update()
        {
            if (!Completed || _centerLight == null) return;
            _centerLight.intensity = Mathf.MoveTowards(_centerLight.intensity, _lightIntensity, _lightIntensity * Time.deltaTime / Mathf.Max(.01f, _lightFadeSeconds));
        }
    }
}
