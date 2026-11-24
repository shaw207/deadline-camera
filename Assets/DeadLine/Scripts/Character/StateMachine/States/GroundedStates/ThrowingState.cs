using UnityEngine;

namespace DeadLine
{
    public class ThrowingState : GroundedState
    {
        public ThrowingState(Character character) : base(character){}

        protected override void ExitState()
        {
            _character.Interaction.Throw();
        }
    }
}
