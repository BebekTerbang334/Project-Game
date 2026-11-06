using System;
using UnityEngine;

namespace Hutan.Ritual
{
    public sealed class PlayerInteractor : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private IncenseInventory _inventory;
        [SerializeField] private float _reach = 2.5f;
        [SerializeField] private LayerMask _raycastMask = ~0;
        private UnityEngine.InputSystem.InputAction _interactAction;
        private bool _interactionRequested;
        private readonly RaycastHit[] _hits = new RaycastHit[64];
        private string _prompt = string.Empty;
        public string Prompt => _prompt;
        public Camera InteractionCamera => _camera;
        public event Action<string> PromptChanged;
        public event Action<string> Feedback;

        private void Awake() { EnsureAction(); }
        private void OnEnable() { EnsureAction(); _interactAction.Enable(); }
        private void OnDisable()
        {
            _interactionRequested = false;
            _interactAction?.Disable();
            SetPrompt(string.Empty);
        }
        private void OnDestroy()
        {
            if (_interactAction == null) return;
            _interactAction.performed -= OnInteract;
            _interactAction.Dispose();
            _interactAction = null;
        }
        private void EnsureAction()
        {
            if (_interactAction != null) return;
            _interactAction = new UnityEngine.InputSystem.InputAction("RitualInteract", UnityEngine.InputSystem.InputActionType.Button, "<Keyboard>/e");
            _interactAction.performed += OnInteract;
        }
        public void ResetSession()
        {
            EnsureAction();
            _interactionRequested = false;
            _interactAction.Disable();
            if (isActiveAndEnabled) _interactAction.Enable();
            SetPrompt(string.Empty);
        }
        private bool CanInteract => _camera != null && _inventory != null && Application.isFocused && Cursor.lockState == CursorLockMode.Locked;
        private void OnInteract(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            _interactionRequested = true;
        }
        private void LateUpdate()
        {
            EnsureAction();
            if (!_interactAction.enabled) _interactAction.Enable();
            bool requested = _interactionRequested;
            _interactionRequested = false;
            IInteractable target = CanInteract ? GetTarget() : null;
            if (requested && target != null)
            {
                string feedback = target.Interact(_inventory);
                Feedback?.Invoke(feedback);
            }
            SetPrompt(target == null ? string.Empty : target.GetPrompt(_inventory));
        }
        private void SetPrompt(string prompt)
        {
            if (_prompt == prompt) return;
            _prompt = prompt;
            PromptChanged?.Invoke(prompt);
        }
        public IInteractable GetTarget()
        {
            if (_camera == null) return null;
            int count = Physics.RaycastNonAlloc(_camera.transform.position, _camera.transform.forward, _hits, _reach, _raycastMask, QueryTriggerInteraction.Collide);
            // Saturation must not accidentally allow interaction through an occluder.
            if (count == _hits.Length) return null;
            Collider nearest = null;
            float distance = float.PositiveInfinity;
            for (int i = 0; i < count; i++)
            {
                var hit = _hits[i];
                if (hit.collider.transform.IsChildOf(transform)) continue;
                var interactable = hit.collider.GetComponentInParent<IInteractable>();
                if (hit.collider.isTrigger && interactable == null) continue;
                if (hit.distance >= distance) continue;
                nearest = hit.collider;
                distance = hit.distance;
            }
            return nearest == null ? null : nearest.GetComponentInParent<IInteractable>();
        }
    }
}
