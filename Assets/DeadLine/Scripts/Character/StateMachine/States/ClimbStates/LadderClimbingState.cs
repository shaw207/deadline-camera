using UnityEngine;

namespace DeadLine
{
    /// <summary>
    /// Estado principal para escalar escaleras - sigue el patrón de PullingState
    /// </summary>
    public class LadderClimbingState : ClimbState
    {
        private const float EXIT_THRESHOLD = 0.1f;
        
        public LadderClimbingState(Character character) : base(character) { }

        protected override void EnterState()
        {
            // Iniciar la interacción con la escalera (similar a PullingState)
            if (_character.Interaction.CurrentInteractables != null && 
                _character.Interaction.CurrentInteractables.Length > 0)
            {
                _character.Interaction.StartInteraction(InteractableType.Ladder);
            }
            
            // Montar al personaje en la escalera
            Ladder currentLadder = GetCurrentLadder();
            if (currentLadder != null)
            {
                _character.Movement.MountLadder(currentLadder);
            }
            else
            {
                Debug.LogWarning("[LadderClimbingState] No hay escalera disponible al entrar al estado");
            }
        }

        protected override void HandleInput()
        {
            // Leer solo input vertical para subir/bajar
            Vector2 input = InputManager.Instance.GetMovementInput();

            // Aplicar movimiento de escalada
            _character.Movement.ApplyLadderClimbing(input);
        }

        protected override void ApplyMovement()
        {
            // El movimiento ya se aplicó en HandleInput (dentro de ApplyLadderClimbing)
            // Este método se deja vacío porque MovementController lo maneja todo
            
            // Actualizar la interacción (similar a PullingState)
            _character.Interaction.UpdateInteraction(Vector3.zero);
        }

        protected override void ExitState()
        {
            // Finalizar la interacción con la escalera
            _character.Interaction.EndInteraction();
            
            // Desmontar de la escalera
            _character.Movement.DismountLadder();
            
        }

        /// <summary>
        /// Helper para obtener la escalera actual del sistema de interacción
        /// </summary>
        private Ladder GetCurrentLadder()
        {
            if (_character.Interaction.ActiveInteractable is Ladder ladder)
            {
                return ladder;
            }
            
            // Fallback: buscar en los interactables actuales
            if (_character.Interaction.CurrentInteractables != null)
            {
                foreach (var interactable in _character.Interaction.CurrentInteractables)
                {
                    if (interactable is Ladder ladderInteractable)
                        return ladderInteractable;
                }
            }
            
            return null;
        }
    }
}