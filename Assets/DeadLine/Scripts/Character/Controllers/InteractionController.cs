using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DeadLine
{
    /// <summary>
    /// Controla la detección e interacción con objetos interactuables
    /// </summary>
    public class InteractionController : MonoBehaviour
    {
        [Header("Detection")]
        [SerializeField] private float detectionDistance = 2f;
        [SerializeField] private float detectionRadius = 0.3f;
        [SerializeField] private LayerMask interactableLayers = -1; // Todas las capas por defecto

        [Header("Hold Position")]
        [SerializeField] private Transform holdPosition;

        private Character _character;
        private IInteractable[] _currentInteractables = null;
        private IInteractable _activeInteractable = null;
        private InteractableType _currentInteractableTypes = InteractableType.None;
        private bool _isInteracting = false;

        // Devolver solo interactables válidos (filtrar objetos Unity destruídos)
        public IInteractable[] CurrentInteractables => _currentInteractables == null ? null : _currentInteractables.Where(i => !(i is UnityEngine.Object uo && uo == null)).ToArray();
        public IInteractable ActiveInteractable => _activeInteractable;
        public InteractableType CurrentInteractableTypes => _currentInteractableTypes;
        public bool IsInteracting => _isInteracting;
        public float DetectionDistance => detectionDistance;

        private void Start()
        {
            _character = GetComponent<Character>();
            if (_character == null)
            {
                Debug.LogError("[InteractionController] No se encontró el componente Character!");
            }
        }

        public void DetectInteractables()
        {
            if (_character == null) return;

            Vector3 origin = _character.transform.position;
            origin.y += _character.Movement.CurrentHeight * 0.5f;

            Vector3 direction = _character.transform.forward;

            RaycastHit[] hits = Physics.RaycastAll(origin, direction, detectionDistance);
            IInteractable[] newInteractables = null;
            InteractableType newTypes = InteractableType.None;
            float closestDistance = float.MaxValue;

            foreach (RaycastHit hit in hits)
            {
                IInteractable[] interactables = hit.collider.GetComponents<IInteractable>();
                
                if (interactables.Length == 0)
                    continue;

                float dist = Vector3.Distance(origin, hit.collider.transform.position);
                if (dist < closestDistance)
                {
                    // Filtrar solo los que pueden interactuar y combinar tipos
                    List<IInteractable> validInteractables = new List<IInteractable>();
                    InteractableType combinedTypes = InteractableType.None;
                    
                    foreach (IInteractable interactable in interactables)
                    {
                        if (interactable != null && interactable.CanInteract(_character))
                        {
                            validInteractables.Add(interactable);
                            combinedTypes |= interactable.Type;
                        }
                    }
                    
                    if (validInteractables.Count > 0)
                    {
                        closestDistance = dist;
                        newInteractables = validInteractables.ToArray();
                        newTypes = combinedTypes;
                    }
                }
            }

            // Verificar si cambió el objeto detectado
            bool hasChanged = false;
            
            if (newInteractables == null && _currentInteractables != null)
                hasChanged = true;
            else if (newInteractables != null && _currentInteractables == null)
                hasChanged = true;
            else if (newInteractables != null && _currentInteractables != null)
            {
                // Verificar si es el mismo GameObject
                GameObject newObj = (newInteractables[0] as Component)?.gameObject;
                GameObject oldObj = (_currentInteractables[0] as Component)?.gameObject;
                hasChanged = newObj != oldObj;
            }

            if (hasChanged)
            {
                // NO terminar interacción si es un objeto sostenido (held) como inventario
                bool isHoldingObject = false;
                if (_activeInteractable != null && _isInteracting)
                {
                    // Verificar si es un ThrowableInteractable sostenido
                    if (_activeInteractable is ThrowableInteractable throwable && throwable.IsHeld)
                    {
                        isHoldingObject = true;
                    }
                    else
                    {
                        // Terminar interacción para otros tipos de interactables
                        _activeInteractable.OnInteractEnd(_character);
                        _activeInteractable = null;
                        _isInteracting = false;
                    }
                }

                // Solo actualizar currentInteractables si no estamos sosteniendo nada
                if (!isHoldingObject)
                {
                    _currentInteractables = newInteractables;
                    _currentInteractableTypes = newTypes;
                    
                    if (_currentInteractables == null)
                    {
                        _currentInteractableTypes = InteractableType.None;
                    }
                }
            }
        }

        public void StartInteraction(InteractableType desiredType)
        {
            if (_currentInteractables == null || _currentInteractables.Length == 0 || _isInteracting) 
                return;

            // Buscar el interactable que coincide con el tipo deseado
            foreach (IInteractable interactable in _currentInteractables)
            {
                if (interactable.Type.HasFlag(desiredType))
                {
                    _activeInteractable = interactable;
                    _activeInteractable.OnInteractStart(_character);
                    _isInteracting = true;
                    return;
                }
            }
            
            Debug.LogWarning($"[Interaction] No se encontró interactable del tipo: {desiredType}");
        }

        public void UpdateInteraction(Vector3 inputDirection)
        {
            if (_activeInteractable == null || !_isInteracting) return;
            _activeInteractable.OnInteracting(_character, inputDirection);
        }

        public void EndInteraction()
        {
            if (_activeInteractable == null || !_isInteracting) return;

            _activeInteractable.OnInteractEnd(_character);
            _activeInteractable = null;
            _isInteracting = false;
        }

        public void HandlePickup()
        {
            if (_currentInteractables == null || _currentInteractables.Length == 0) 
                return;

            // Buscar el interactable de tipo Pickup
            foreach (IInteractable interactable in _currentInteractables)
            {
                if (interactable.Type.HasFlag(InteractableType.Pickup))
                {
                    _activeInteractable = interactable;
                    _activeInteractable.OnInteractStart(_character);
                    _isInteracting = true;
                    return;
                }
            }
            
            Debug.LogWarning("[Interaction] No se encontró interactable del tipo: Pickup");
        }

        public void Throw()
        {
            if (_activeInteractable == null) return;

            if (_activeInteractable.Type.HasFlag(InteractableType.Throwable))
            {
                // Llamar al m\u00e9todo Throw espec\u00edfico del ThrowableInteractable
                if (_activeInteractable is ThrowableInteractable throwable)
                {
                    throwable.Throw(_character);
                    _activeInteractable = null;
                    _isInteracting = false;
                }
                else
                {
                    Debug.LogWarning("[Interaction] El interactable es Throwable pero no es ThrowableInteractable!");
                }
            }
            else
            {
                Debug.LogWarning("[Interaction] El interactable activo no es de tipo Throwable!");
            }
        }

        public Transform GetHoldPosition()
        {
            return holdPosition;
        }

        private void OnDrawGizmos()
        {
            // Dibuja el raycast de detección siempre en la escena
            if (_character == null) return;

            Vector3 origin = _character.transform.position;
            origin.y += _character.Movement.CurrentHeight * 0.5f;

            Vector3 direction = _character.transform.forward;
            Vector3 endPoint = origin + direction * detectionDistance;

            Gizmos.color = _currentInteractables != null && _currentInteractables.Length > 0 ? Color.green : Color.yellow;
            Gizmos.DrawWireSphere(origin, detectionRadius);
            Gizmos.DrawLine(origin, endPoint);
            Gizmos.DrawWireSphere(endPoint, detectionRadius);

            if (_currentInteractables != null && _currentInteractables.Length > 0 && _currentInteractables[0] is Component component)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(origin, component.transform.position);
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Puedes dejarlo vacío o reutilizar la lógica si quieres un comportamiento diferente al seleccionar
        }
    }
}