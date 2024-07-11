namespace Fifbox.Game.Player.StateMachine.States.OnGroundSubStates
{
    public class WalkingState : OnGroundSubState
    {
        public override float JumpForce => Player.Config.walkJumpForce;
        public override float MoveSpeed => Player.Config.walkSpeed;
        public override float Acceleration => Player.Config.walkAcceleration;
        public override float Deceleration => Player.Config.walkDeceleration;

        public override OnGroundSubState GetNextState()
        {
            if (Player.Inputs.wantsToCrouch) return new CrouchingState();

            if (Player.Inputs.moveVector.magnitude > 0f)
            {
                if (Player.Inputs.wantsToRun) return new RunningState();
                else return null;
            }

            return new IdlingState();
        }
    }
}