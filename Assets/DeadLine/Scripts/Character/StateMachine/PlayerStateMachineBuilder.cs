using System;
using UnityEngine;

namespace DeadLine
{
    public class PlayerStateMachineBuilder
    {
        private readonly Character _character;
        private readonly StateMachine _stateMachine;
        private readonly InputManager _input;

        public PlayerStateMachineBuilder(Character character)
        {
            _character = character;
            _stateMachine = new StateMachine();
            _input = InputManager.Instance;
        }

        public StateMachine Build()
        {
            // ==================== STATE CREATION ====================
            IState walkState = new WalkState(_character);
            IState jumpState = new JumpState(_character);
            IState idleState = new IdleState(_character);
            IState runState = new RunState(_character);
            IState airbornState = new AirborneState(_character);
            IState ledgeHangState = new LedgeHangState(_character);
            IState ledgeClimbState = new LedgeClimbingState(_character);
            IState crouchingState = new CrouchingState(_character);
            IState crouchIdleState = new CrouchIdleState(_character);
            IState standingUpState = new StandingUpState(_character);
            IState crouchWalkState = new CrouchWalkState(_character);
            IState pushingState = new PushingState(_character);
            IState pullingState = new PullingState(_character);
            IState throwingState = new ThrowingState(_character);
            
            // Ladder state - solo necesitamos uno
            IState ladderClimbingState = new LadderClimbingState(_character);

            // ==================== EVENT PREDICATES ====================
            EventPredicate crouchEventPredicate = new EventPredicate();
            EventPredicate pullingPressedEventPredicate = new EventPredicate();
            EventPredicate pullingCanceledEventPredicate = new EventPredicate();
            EventPredicate ladderClimbPressedEventPredicate = new EventPredicate();
            EventPredicate pickUpPressedEventPredicate = new EventPredicate();
            EventPredicate throwPressedEventPredicate = new EventPredicate();
            EventPredicate throwCanceledEventPredicate = new EventPredicate();
            
            _input.CrouchPressed += crouchEventPredicate.Trigger;
            _input.InteractPressed += pullingPressedEventPredicate.Trigger;
            _input.InteractPressed += ladderClimbPressedEventPredicate.Trigger;
            _input.InteractCanceled += pullingCanceledEventPredicate.Trigger;
            _input.InteractPressed += pickUpPressedEventPredicate.Trigger;
            _input.ThrowPressed += throwPressedEventPredicate.Trigger;
            _input.ThrowCanceled += throwCanceledEventPredicate.Trigger;
            

            // ==================== BASIC MOVEMENT TRANSITIONS ====================
            AddTransition(idleState, walkState, new FunctionPredicate(() => _input.IsMovementPressed));
            AddTransition(runState, walkState, new FunctionPredicate(() => !_input.IsSprintPressed && _input.IsMovementPressed));
            AddTransition(new[] { walkState, runState}, idleState, new FunctionPredicate(() => !_input.IsMovementPressed));
            AddTransition(new[] { idleState, walkState }, runState, new FunctionPredicate(() => _input.IsSprintPressed && _input.IsMovementPressed));

            // ==================== JUMP TRANSITIONS ====================
            AddTransition(new[] { idleState, walkState, runState}, jumpState, new FunctionPredicate(() => _input.IsJumpPressed && _character.Movement.IsGrounded));
            AddTransition(airbornState, idleState, new FunctionPredicate(() => _character.Movement.IsGrounded));

            // ==================== LEDGE TRANSITIONS ====================
            AddTransition(new[] { jumpState, airbornState }, ledgeHangState, new FunctionPredicate(() => _character.Movement.CanGrabLedge));
            AddTransition(ledgeHangState, ledgeClimbState, new FunctionPredicate(() => _input.IsJumpPressed));
            AddTransition(ledgeClimbState, idleState, new FunctionPredicate(() => _character.Movement.IsGrounded));

            // ==================== CROUCH TRANSITIONS ====================
            AddTransition(new[] { walkState, runState, idleState}, crouchingState, crouchEventPredicate);
            AddTransition(crouchingState, crouchIdleState, new FunctionPredicate(() => _character.Movement.IsAlreadyCrouched));
            AddTransition(new [] { crouchIdleState, crouchWalkState }, standingUpState, new FunctionPredicate(() => crouchEventPredicate.Evaluate() && _character.Movement.CanStandUp));
            AddTransition(standingUpState, idleState, new FunctionPredicate(() => _character.Movement.IsAlreadyStandUp));
            AddTransition(crouchIdleState, idleState, crouchEventPredicate);
            AddTransition(crouchIdleState, crouchWalkState, new FunctionPredicate(() => _input.IsMovementPressed));
            AddTransition(crouchWalkState, crouchIdleState, new FunctionPredicate(() => !_input.IsMovementPressed));

            // ==================== PUSH/PULL TRANSITIONS ====================
            AddTransition(new[]{idleState, walkState}, pullingState, new FunctionPredicate(() => 
                pullingPressedEventPredicate.Evaluate() && _character.Interaction.CurrentInteractableTypes.HasFlag(InteractableType.Pull)));
            AddTransition(pullingState, idleState, new FunctionPredicate(() => pullingCanceledEventPredicate.Evaluate()));
            
            AddTransition(new[]{idleState, walkState}, pushingState, new FunctionPredicate(() => 
                _input.IsMovementPressed && _character.Interaction.CurrentInteractableTypes.HasFlag(InteractableType.Push)));
            AddTransition(pushingState, idleState, new FunctionPredicate(() => !_character.Interaction.IsInteracting));
            
            // ==================== LADDER TRANSITIONS ====================
            
            // Entrar a la escalera desde estados en el suelo (similar a Pull)
            AddTransition(new[]{idleState, walkState, runState, airbornState}, ladderClimbingState, new FunctionPredicate(() => 
                ladderClimbPressedEventPredicate.Evaluate() && 
                _character.Interaction.CurrentInteractableTypes.HasFlag(InteractableType.Ladder)));
            
            // Salir de la escalera cuando llega al tope (transición directa a airborne para que caiga suavemente)
            AddTransition(ladderClimbingState, airbornState, new FunctionPredicate(() => 
                _character.Movement.HasReachedLadderTop() && _input.IsMovementPressed));
            
            // Salir de la escalera cuando llega al fondo Y está intentando bajar (transición a idle porque ya está en el suelo)
            AddTransition(ladderClimbingState, idleState, new FunctionPredicate(() => 
                _character.Movement.HasReachedLadderBottom() && _input.GetMovementInput().y < -0.1f));
            
            // Saltar desde la escalera en cualquier momento
            AddTransition(ladderClimbingState, airbornState, new FunctionPredicate(() => 
                _input.IsJumpPressed && _character.Movement.IsOnLadder));
            
            // Cancelar interacción con la escalera (presionar E de nuevo para soltarse)
            AddTransition(ladderClimbingState, airbornState, new FunctionPredicate(() => 
                ladderClimbPressedEventPredicate.Evaluate()));

            // ==================== PICK UP, DROP && THROWING TRANSITIONS ====================
            
            // Lanzar objeto si está sosteniendo uno
            AddTransition(new[]{ idleState, walkState, runState }, throwingState, new FunctionPredicate(() => 
                _character.Interaction.CurrentInteractableTypes.HasFlag(InteractableType.Throwable)
                && throwPressedEventPredicate.Evaluate()));

            AddAnyTransition(idleState, new FunctionPredicate(() => 
                pickUpPressedEventPredicate.Evaluate() && _character.Interaction.CurrentInteractableTypes.HasFlag(InteractableType.Pickup)
                && HandlePickupAndReturnFalse()));

            AddTransition(throwingState, idleState, new FunctionPredicate(() => 
                throwCanceledEventPredicate.Evaluate()));

            // ==================== ANY TRANSITIONS ====================
            AddAnyTransitionExcept(idleState, new [] { pullingState, crouchIdleState, crouchingState, ladderClimbingState }, 
                new FunctionPredicate(() => _character.Movement.IsGrounded && !_input.IsMovementPressed));
            
            AddAnyTransitionExcept(airbornState, new [] { ledgeHangState, ladderClimbingState }, 
                new FunctionPredicate(() => !_character.Movement.IsGrounded && !_input.IsJumpPressed));
            
            // ==================== INITIAL STATE ====================
            _stateMachine.SetState(idleState);

            return _stateMachine;
        }

        private void AddTransition(IState from, IState to, IPredicate condition) => _stateMachine.AddTransition(from, to, condition);
        
        private void AddTransition(IState[] fromStates, IState to, IPredicate condition)
        {
            foreach (IState state in fromStates)
            {
                AddTransition(state, to, condition);
            }
        }

        private void AddAnyTransitionExcept(IState to, IState[] except, IPredicate condition) => _stateMachine.AddAnyTransitionExcept(to, except, condition);
        private void AddAnyTransitionExcept(IState to, IState except, IPredicate condition) => _stateMachine.AddAnyTransitionExcept(to, new[] {except}, condition);
        private void AddAnyTransition(IState to, IPredicate condition) => _stateMachine.AddAnyTransition(to, condition);

        private bool HandlePickupAndReturnFalse()
        {
            _character.Interaction.HandlePickup();
            return false;
        }
    }
}