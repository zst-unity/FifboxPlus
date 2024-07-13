namespace Fifbox.Player.StateMachine.States.OnGroundSubStates
{
    public class RunningState : OnGroundSubState
    {
        public override float JumpForce => Player.Config.runJumpForce;
        public override float MoveSpeed => Player.Config.runSpeed;
        public override float Acceleration => Player.Config.runAcceleration;
        public override float Deceleration => Player.Config.runDeceleration;

        public override OnGroundSubState GetNextState()
        {
            if (PlayerInputs.WantsToCrouch) return new RunningState();

            if (PlayerInputs.MoveVector.magnitude > 0f)
            {
                if (PlayerInputs.WantsToRun) return null;
                else return new WalkingState();
            }

            return new IdlingState();
        }
    }
}