using Fifbox.Game.Player.StateMachine.States;

namespace Fifbox.Game.Player.StateMachine
{
    public class PlayerStateMachine<T, T1> where T : PlayerStateBase<T> where T1 : T, new()
    {
        public T CurrentState { get; private set; }

        public readonly Player player;
        public readonly PlayerInputsController playerInputs;

        public PlayerStateMachine(Player player, PlayerInputsController playerInputs)
        {
            this.player = player;
            this.playerInputs = playerInputs;
        }

        public void Start()
        {
            CurrentState = new T1();
            CurrentState.Enter(player, playerInputs);
        }

        public void Update()
        {
            CurrentState.OnUpdate();

            var nextState = CurrentState.GetNextState();
            if (nextState == null) return;

            CurrentState.OnExit();
            CurrentState = nextState;
            CurrentState.Enter(player, playerInputs);
        }

        public void LateUpdate()
        {
            CurrentState.OnLateUpdate();
        }

        public void Stop()
        {
            CurrentState.OnExit();
        }
    }
}