namespace Fifbox.Game.Player.StateMachine.States
{
    public abstract class PlayerState : PlayerStateBase<PlayerState, PlayerStatesData> { }

    public abstract class PlayerStateBase<T, T1> where T : PlayerStateBase<T, T1> where T1 : PlayerStatesDataBase, new()
    {
        public Player Player { get; private set; }
        public PlayerInputsController PlayerInputs { get; private set; }
        public T1 Data { get; private set; }

        public void Enter(Player player, PlayerInputsController playerInputs, T1 statesData)
        {
            Player = player;
            PlayerInputs = playerInputs;
            Data = statesData;
            OnEnter();
        }

        protected virtual void OnEnter() { }
        public virtual void OnExit() { }
        public virtual void OnUpdate() { }
        public virtual void OnLateUpdate() { }
        public abstract T GetNextState();
    }

    public partial class PlayerStatesData : PlayerStatesDataBase
    {
        public float jumpBufferTimer;
    }

    public abstract class PlayerStatesDataBase { }
}