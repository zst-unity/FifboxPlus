using Fifbox.Game.Player.StateMachine.States;

namespace Fifbox.Game.Player.StateMachine
{
    public class PlayerStateMachine<T, T1, T2> where T : PlayerStateBase<T, T2> where T1 : T, new() where T2 : PlayerStatesDataBase, new()
    {
        public T CurrentState { get; private set; }

        public readonly Player player;
        public readonly PlayerInputsController playerInputs;

        private T2 _statesData;

        public PlayerStateMachine(Player player, PlayerInputsController playerInputs)
        {
            this.player = player;
            this.playerInputs = playerInputs;

            _statesData = new();
        }

        public void Start()
        {
            CurrentState = new T1();
            CurrentState.Enter(player, playerInputs, _statesData);
        }

        public void Update()
        {
            CurrentState.OnUpdate();

            var nextState = CurrentState.GetNextState();
            if (nextState == null) return;

            CurrentState.OnExit();
            CurrentState = nextState;
            CurrentState.Enter(player, playerInputs, _statesData);
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