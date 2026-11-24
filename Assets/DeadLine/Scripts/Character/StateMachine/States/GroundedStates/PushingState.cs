namespace DeadLine
{
    public class PushingState : GroundedState
    {
        public PushingState(Character character) : base(character){}

        protected override void EnterState()
        {
            if (_character.Interaction.CurrentInteractables != null && _character.Interaction.CurrentInteractables.Length > 0)
                _character.Interaction.StartInteraction(InteractableType.Push);
        }


        protected override void ApplyMovement()
        {
            _character.Movement.WalkWhileMovingObject(InputManager.Instance.GetMovementInput(), _character.Interaction.ActiveInteractable.Weight);
            _character.Movement.ApplyVerticalMove();
            _character.Interaction.DetectInteractables();
            _character.Interaction.UpdateInteraction(InputManager.Instance.GetMovementInput());
        }

        protected override void ExitState()
        {
            if (_character.Interaction.IsInteracting)
                _character.Interaction.EndInteraction();
        }
    }
}
