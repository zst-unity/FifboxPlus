namespace Fifbox.Player.StateMachine.States.OnGroundSubStates
{
    public class WalkingState : OnGroundSubState
    {
        public override float JumpForce => Player.Config.walkJumpForce;
        public override float MoveSpeed => Player.Config.walkSpeed;
        public override float Acceleration => Player.Config.walkAcceleration;
        public override float Deceleration => Player.Config.walkDeceleration;

        public override OnGroundSubState GetNextState()
        {
            if (PlayerInputs.WantsToCrouch) return new CrouchingState();

            if (PlayerInputs.MoveVector.magnitude > 0f)
            {
                if (PlayerInputs.WantsToRun) return new RunningState();
                else return null;
            }

            return new IdlingState();
        }
    }
}