namespace Fifbox.Game.Player.StateMachine.States.OnGroundSubStates
{
    public class RunningState : OnGroundSubState
    {
        public override float JumpForce => Player.ConfigToUse.runJumpForce;
        public override float MoveSpeed => Player.ConfigToUse.runSpeed;
        public override float Acceleration => Player.ConfigToUse.runAcceleration;
        public override float Deceleration => Player.ConfigToUse.runDeceleration;

        public override void Enter(Player player)
        {
            Player = player;
        }

        public override void Exit()
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

        public override void Update()
        {

        }
    }
}