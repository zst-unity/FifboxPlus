namespace Fifbox.Game.Player.StateMachine.States.OnGroundSubStates
{
    public class WalkingState : OnGroundSubState
    {
        public override float JumpForce => Player.ConfigToUse.walkJumpForce;
        public override float MoveSpeed => Player.ConfigToUse.walkSpeed;
        public override float Acceleration => Player.ConfigToUse.walkAcceleration;
        public override float Deceleration => Player.ConfigToUse.walkDeceleration;

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
                else return null;
            }

            return new IdlingState();
        }

        public override void Update()
        {

        }
    }
}