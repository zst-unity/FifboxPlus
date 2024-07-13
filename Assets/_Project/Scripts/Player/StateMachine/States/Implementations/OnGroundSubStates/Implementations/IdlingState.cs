namespace Fifbox.Player.StateMachine.States.OnGroundSubStates
{
    public class IdlingState : OnGroundSubState
    {
        public override float JumpForce => Player.Config.walkJumpForce;
        public override float MoveSpeed => 0f;
        public override float Acceleration => 0f;
        public override float Deceleration => 0f;

        public override OnGroundSubState GetNextState()
        {
            if (PlayerInputs.WantsToCrouch) return new CrouchingState();

            if (PlayerInputs.MoveVector.magnitude > 0f)
            {
                if (PlayerInputs.WantsToRun) return new RunningState();
                else return new WalkingState();
            }

            return null;
        }
    }
}