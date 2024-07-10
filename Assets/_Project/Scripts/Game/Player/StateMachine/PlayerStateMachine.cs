using Fifbox.Game.Player.StateMachine.States;

namespace Fifbox.Game.Player.StateMachine
{
    public class PlayerStateMachine<T, T1> where T : PlayerStateBase<T> where T1 : T, new()
    {
        public T CurrentState { get; private set; }

        public readonly Player player;

        public PlayerStateMachine(Player player)
        {
            this.player = player;
        }

        public void Start()
        {
            CurrentState = new T1();
            CurrentState.Enter(player);
        }

        public void Update()
        {
            CurrentState.Update();

            var nextState = CurrentState.GetNextState();
            if (nextState == null) return;

            CurrentState.Exit();
            CurrentState = nextState;
            CurrentState.Enter(player);
        }

        public void LateUpdate()
        {
            CurrentState.LateUpdate();
        }

        public void Stop()
        {
            CurrentState.Exit();
        }
    }
}