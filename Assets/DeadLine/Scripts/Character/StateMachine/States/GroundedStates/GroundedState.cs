using UnityEngine;

namespace DeadLine
{
    public abstract class GroundedState : BaseState
    {
        protected GroundedState(Character character) : base(character) {}

        protected override void ApplyPhysics()
        {
            _character.Movement.ApplyGroundedGravity();
        }

        protected override void ApplyMovement()
        {
            _character.Movement.Walk(InputManager.Instance.GetMovementInput());
            _character.Movement.ApplyVerticalMove();
            _character.Interaction.DetectInteractables();
        }
    }
}