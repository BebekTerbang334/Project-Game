using UnityEngine;

namespace Hutan.Ritual
{
    public sealed class RitualSpawnAnchor : MonoBehaviour
    {
        public enum AnchorKind { Incense, Chest, RitualCenter }
        [SerializeField] private AnchorKind _kind;
        [SerializeField] private float _clearance = .75f;
        public AnchorKind Kind => _kind;
        public float Clearance => _clearance;
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = _kind == AnchorKind.RitualCenter ? Color.cyan : _kind == AnchorKind.Chest ? Color.yellow : new Color(1f,.5f,.2f);
            Gizmos.DrawWireSphere(transform.position, _clearance);
        }
    }
}
