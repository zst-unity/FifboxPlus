namespace Fifbox.Game.Player.StateMachine.States.OnGroundSubStates
{
    public class RunningState : OnGroundSubState
    {
        public override float JumpForce => Player.Config.runJumpForce;
        public override float MoveSpeed => Player.Config.runSpeed;
        public override float Acceleration => Player.Config.runAcceleration;
        public override float Deceleration => Player.Config.runDeceleration;

        protected override void OnEnter()
        {

        }

        public override void OnExit()
        {

        }

        public override OnGroundSubState GetNextState()
        {
            if (Player.Inputs.wantsToCrouch) return new RunningState();

            if (Player.Inputs.moveVector.magnitude > 0f)
            {
                if (Player.Inputs.wantsToRun) return null;
                else return new WalkingState();
            }

            return new IdlingState();
        }

        public override void OnLateUpdate()
        {

        }

        public override void OnUpdate()
        {

        }
    }
}