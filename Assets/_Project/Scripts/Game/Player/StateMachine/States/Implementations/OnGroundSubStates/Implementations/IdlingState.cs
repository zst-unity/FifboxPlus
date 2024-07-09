namespace Fifbox.Game.Player.StateMachine.States.OnGroundSubStates
{
    public class IdlingState : OnGroundSubState
    {
        public override float JumpForce => Player.ConfigToUse.walkJumpForce;
        public override float MoveSpeed => 0f;
        public override float Acceleration => 0f;
        public override float Deceleration => 0f;

        public override void Enter(Player player)
        {
            Player = player;
        }

        public override void Exit()
        {

        }

        public override OnGroundSubState GetNextState()
        {
            if (Player.Inputs.wantsToCrouch) return new CrouchingState();

            if (Player.Inputs.moveVector.magnitude > 0f)
            {
                if (Player.Inputs.wantsToRun) return new RunningState();
                else return new WalkingState();
            }

            return null;
        }

        public override void Update()
        {

        }
    }
}