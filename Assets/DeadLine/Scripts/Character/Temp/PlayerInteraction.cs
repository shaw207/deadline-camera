using UnityEngine;

namespace DeadLine
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.InputSystem;

    public class PlayerInteraction : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private InputActionReference interactAction;

        private readonly HashSet<InteractableObjectsBase> _candidates = new();

        private void OnEnable()
        {
            interactAction.action.performed += OnInteract;
            interactAction.action.Enable();
        }

        private void OnDisable()
        {
            interactAction.action.performed -= OnInteract;
            interactAction.action.Disable();
        }

        public void Register(InteractableObjectsBase obj)
        {
            _candidates.Add(obj);
        }

        public void Unregister(InteractableObjectsBase obj)
        {
            _candidates.Remove(obj);
        }

        private void OnInteract(InputAction.CallbackContext ctx)
        {
            if (_candidates.Count == 0)
                return;

            foreach (var obj in _candidates)
            {
                if (obj == null) continue;

                obj.Interact();
            }
        }
    }

}
