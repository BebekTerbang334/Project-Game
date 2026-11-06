using System;
using UnityEngine;

namespace Hutan.Ritual
{
    public sealed class IncenseInventory : MonoBehaviour
    {
        [SerializeField] private GameObject _carriedVisual;
        public bool HasIncense { get; private set; }
        public bool HasTirta { get; private set; }
        public event Action Changed;

        public void ResetSession()
        {
            HasIncense = false;
            HasTirta = false;
            Refresh();
        }

        public bool TryTakeIncense()
        {
            if (HasIncense) return false;
            HasIncense = true;
            Refresh();
            return true;
        }

        public bool TryPlaceIncense()
        {
            if (!HasIncense) return false;
            HasIncense = false;
            Refresh();
            return true;
        }

        public bool TryTakeTirta()
        {
            if (HasTirta || HasIncense) return false;
            HasTirta = true;
            Refresh();
            return true;
        }

        private void Refresh()
        {
            if (_carriedVisual != null) _carriedVisual.SetActive(HasIncense);
            Changed?.Invoke();
        }
    }
}
