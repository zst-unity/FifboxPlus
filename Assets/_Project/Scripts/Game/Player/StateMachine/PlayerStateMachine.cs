using Fifbox.Game.Player.StateMachine.States;

namespace Fifbox.Game.Player.StateMachine
{
    public class PlayerStateMachine<T, T1, T2> where T : PlayerStateBase<T, T2> where T1 : T, new() where T2 : PlayerStatesDataBase, new()
    {
        public T CurrentState { get; private set; }

        public readonly Player player;

        private readonly PlayerInputsController _playerInputs;
        private readonly PlayerHeightsController _playerHeights;
        private readonly T2 _statesData;

        public PlayerStateMachine(Player player, PlayerInputsController playerInputs, PlayerHeightsController playerHeights)
        {
            this.player = player;

            _playerInputs = playerInputs;
            _playerHeights = playerHeights;
            _statesData = new();
        }

        public void Start()
        {
            CurrentState = new T1();
            CurrentState.Enter(player, _playerInputs, _playerHeights, _statesData);
        }

        public void Update()
        {
            CurrentState.OnUpdate();

            var nextState = CurrentState.GetNextState();
            if (nextState == null) return;

            CurrentState.OnExit();
            CurrentState = nextState;
            CurrentState.Enter(player, _playerInputs, _playerHeights, _statesData);
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