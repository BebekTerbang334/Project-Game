using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Hutan.Ritual
{
    public sealed class RitualSpawner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Terrain _terrain;
        [SerializeField] private Transform _player;
        [SerializeField] private Transform _ghost;
        [SerializeField] private IncenseInventory _inventory;
        [SerializeField] private PlayerInteract _interactor;
        [SerializeField] private RitualManager _manager;
        [SerializeField] private RitualSpawnAnchor[] _anchors;
        [SerializeField] private GameObject _incensePrefab;
        [SerializeField] private GameObject _chestPrefab;
        [SerializeField] private GameObject _stonePrefab;
        [SerializeField] private GameObject _centerPrefab;
        [Header("Placement")]
        [SerializeField] private float _ringRadius = 3.2f;
        [SerializeField] private float _minSeparation = 8f;
        [SerializeField] private float _minPlayerDistance = 5f;
        [SerializeField] private float _maxSlope = 25f;
        [SerializeField] private int _attempts = 64;
        [SerializeField] private bool _useFixedSeed;
        [SerializeField] private int _seed = 1234;
        [Tooltip("One prevalidated fallback center, chest and three incense positions.")]
        [SerializeField] private Vector3[] _fallbackLayout;
        private readonly Collider[] _overlaps = new Collider[64];
        private readonly List<Bounds> _decorationBounds = new List<Bounds>();
        private Transform _runtimeRoot;
        private Vector3 _pathOrigin;
        public int SessionSeed { get; private set; }
        public Transform RuntimeRoot => _runtimeRoot;
        public bool Ready { get; private set; }

        // Runs even when both Domain Reload and Scene Reload are disabled.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void ResetLoadedScenes()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded) continue;
                foreach (var root in scene.GetRootGameObjects())
                    foreach (var spawner in root.GetComponentsInChildren<RitualSpawner>(true))
                        if (spawner.isActiveAndEnabled) spawner.BeginSession();
            }
        }

        public void BeginSession()
        {
            Ready = false;
            if (_runtimeRoot != null)
            {
                _runtimeRoot.gameObject.SetActive(false);
                Destroy(_runtimeRoot.gameObject);
                _runtimeRoot = null;
            }
            if (_terrain == null || _player == null || _manager == null || _inventory == null || _interactor == null ||
                _incensePrefab == null || _chestPrefab == null || _stonePrefab == null || _centerPrefab == null)
            {
                Debug.LogError("Referensi RitualSpawner belum lengkap", this);
                return;
            }
            _inventory.ResetSession();
            _interactor.ResetSession();
            Physics.SyncTransforms();
            CacheDecorationBounds();
            SessionSeed = _useFixedSeed ? _seed : Guid.NewGuid().GetHashCode();
            if (!TryCreateLayout(SessionSeed, out var layout, out float angle))
            {
                Debug.LogError("Tidak ada layout ritual aman; periksa kandidat lokasi dan fallback", this);
                return;
            }
            var generated = new GameObject("Ritual Runtime");
            generated.transform.SetParent(transform, false);
            _runtimeRoot = generated.transform;
            var center = Instantiate(_centerPrefab, layout[0], Quaternion.identity, _runtimeRoot);
            Instantiate(_chestPrefab, layout[1], Quaternion.Euler(0, angle, 0), _runtimeRoot);
            for (int i = 2; i < 5; i++) Instantiate(_incensePrefab, layout[i], Quaternion.identity, _runtimeRoot);
            var stones = new RitualStone[RitualManager.RequiredIncense];
            for (int i = 0; i < stones.Length; i++)
            {
                var position = RingPoint(layout[0], angle, i);
                var stone = Instantiate(_stonePrefab, position, Quaternion.Euler(0, angle + i * 360f / stones.Length, 0), _runtimeRoot);
                stones[i] = stone.GetComponent<RitualStone>();
            }
            _manager.Initialize(stones, center.GetComponentInChildren<Light>(), _runtimeRoot, layout[0] + Vector3.up * .22f);
            Ready = true;
        }

        private void CacheDecorationBounds()
        {
            _decorationBounds.Clear();
            foreach (var root in gameObject.scene.GetRootGameObjects())
            {
                if (root == gameObject || (_player != null && root.transform == _player) || (_ghost != null && root.transform == _ghost)) continue;
                foreach (var renderer in root.GetComponentsInChildren<MeshRenderer>())
                    if (renderer.enabled && renderer.GetComponent<Collider>() == null) _decorationBounds.Add(renderer.bounds);
            }
        }

        public bool PrepareValidation()
        {
            if (_terrain == null || _player == null) return false;
            CacheDecorationBounds();
            var origin = Ground(_player.position);
            if (!UnityEngine.AI.NavMesh.SamplePosition(origin, out var hit, 2f, UnityEngine.AI.NavMesh.AllAreas)) return false;
            _pathOrigin = hit.position;
            return true;
        }

        public bool TryCreateLayout(int seed, out Vector3[] layout, out float angle)
        {
            layout = null;
            angle = 0f;
            if (_anchors == null || !PrepareValidation()) return false;
            var random = new System.Random(seed);
            var centers = new List<RitualSpawnAnchor>();
            var chests = new List<RitualSpawnAnchor>();
            var incense = new List<RitualSpawnAnchor>();
            foreach (var anchor in _anchors)
            {
                if (anchor == null) continue;
                if (anchor.Kind == RitualSpawnAnchor.AnchorKind.RitualCenter) centers.Add(anchor);
                else if (anchor.Kind == RitualSpawnAnchor.AnchorKind.Chest) chests.Add(anchor);
                else incense.Add(anchor);
            }
            for (int attempt = 0; attempt < _attempts; attempt++)
            {
                if (centers.Count == 0 || chests.Count == 0 || incense.Count < 3) break;
                angle = (float)random.NextDouble() * 360f;
                var center = Ground(centers[random.Next(centers.Count)].transform.position);
                if (!ValidateRing(center, angle)) continue;
                var positions = new List<Vector3> { center };
                var chest = Ground(chests[random.Next(chests.Count)].transform.position);
                if (!FarEnough(chest, positions) || !ValidatePosition(chest, 1.1f)) continue;
                positions.Add(chest);
                var choices = new List<RitualSpawnAnchor>(incense);
                while (choices.Count > 0 && positions.Count < 5)
                {
                    int index = random.Next(choices.Count);
                    var anchor = choices[index]; choices.RemoveAt(index);
                    var p = Ground(anchor.transform.position);
                    if (FarEnough(p, positions) && ValidatePosition(p, anchor.Clearance)) positions.Add(p);
                }
                if (positions.Count == 5) { layout = positions.ToArray(); return true; }
            }
            // Bounded fallback also receives current collision/path validation.
            if (_fallbackLayout == null || _fallbackLayout.Length != 5) return false;
            angle = 0f;
            var fallback = new Vector3[5];
            var accepted = new List<Vector3>();
            for (int i = 0; i < 5; i++)
            {
                fallback[i] = Ground(_fallbackLayout[i]);
                if (i == 0 ? !ValidateRing(fallback[i], angle) : !FarEnough(fallback[i], accepted) || !ValidatePosition(fallback[i], i == 1 ? 1.1f : .75f)) return false;
                accepted.Add(fallback[i]);
            }
            layout = fallback;
            return true;
        }
        public bool ValidateCandidate(Vector3 point, float clearance)
        {
            return ValidatePosition(Ground(point), clearance);
        }

        public bool ValidateRingCandidate(Vector3 center, float angle)
        {
            return ValidateRing(Ground(center), angle);
        }

        private bool FarEnough(Vector3 point, List<Vector3> positions)
        {
            if (HorizontalDistance(point, _player.position) < _minPlayerDistance) return false;
            foreach (var p in positions) if (HorizontalDistance(point, p) < _minSeparation) return false;
            return true;
        }
        private static float HorizontalDistance(Vector3 a, Vector3 b) { a.y = 0f; b.y = 0f; return Vector3.Distance(a, b); }
        public Vector3 RingPoint(Vector3 center, float angle, int index)
        {
            float radians = (angle + index * 360f / RitualManager.RequiredIncense) * Mathf.Deg2Rad;
            return Ground(center + new Vector3(Mathf.Cos(radians), 0, Mathf.Sin(radians)) * _ringRadius);
        }
        private bool ValidateRing(Vector3 center, float angle)
        {
            if (HorizontalDistance(center, _player.position) < _minPlayerDistance + _ringRadius || !ValidatePosition(center, .8f)) return false;
            for (int i = 0; i < RitualManager.RequiredIncense; i++)
            {
                var p = RingPoint(center, angle, i);
                if (!ValidatePosition(p, 1.15f) || Mathf.Abs(p.y - center.y) > .65f) return false;
            }
            return true;
        }
        public Vector3 Ground(Vector3 point)
        {
            point.y = _terrain.SampleHeight(point) + _terrain.transform.position.y;
            return point;
        }
        private bool ValidatePosition(Vector3 point, float clearance)
        {
            var local = point - _terrain.transform.position;
            var size = _terrain.terrainData.size;
            if (local.x < 3 || local.z < 3 || local.x > size.x - 3 || local.z > size.z - 3) return false;
            if (_terrain.terrainData.GetSteepness(local.x / size.x, local.z / size.z) > _maxSlope) return false;
            int count = Physics.OverlapSphereNonAlloc(point + Vector3.up * 1f, clearance, _overlaps, ~0, QueryTriggerInteraction.Ignore);
            if (count == _overlaps.Length) return false;
            for (int i = 0; i < count; i++)
            {
                var c = _overlaps[i];
                if (c is TerrainCollider || c.transform.IsChildOf(transform) || (_ghost != null && c.transform.IsChildOf(_ghost))) continue;
                return false;
            }
            var space = new Bounds(point + Vector3.up, new Vector3(clearance * 2f, 2f, clearance * 2f));
            foreach (var bounds in _decorationBounds) if (bounds.Intersects(space)) return false;
            if (!UnityEngine.AI.NavMesh.SamplePosition(point, out var navHit, 1f, UnityEngine.AI.NavMesh.AllAreas) || Mathf.Abs(navHit.position.y - point.y) > .65f) return false;
            var path = new UnityEngine.AI.NavMeshPath();
            return UnityEngine.AI.NavMesh.CalculatePath(_pathOrigin, navHit.position, UnityEngine.AI.NavMesh.AllAreas, path) && path.status == UnityEngine.AI.NavMeshPathStatus.PathComplete;
        }
    }
}
