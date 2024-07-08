using Fifbox.Game.Player.StateMachine.States.OnGroundSubStates;

namespace Fifbox.Game.Player.StateMachine.States
{
    public class OnGroundState : PlayerState
    {
        public PlayerStateMachine<OnGroundSubState, IdlingState> StateMachine { get; private set; }

        public override void Enter(Player player)
        {
            StateMachine = new(player);
            StateMachine.Start();
        }

        public override void Exit()
        {
            StateMachine.Stop();
        }

        public override PlayerState GetNextState()
        {
            return null;
        }

        public override void Update()
        {
            StateMachine.Update();
        }
    }
}