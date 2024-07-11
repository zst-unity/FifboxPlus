namespace Fifbox.Game.Player.StateMachine.States
{
    public abstract class PlayerState : PlayerStateBase<PlayerState>
    {
        public abstract void TryJump();
    }

    public abstract class PlayerStateBase<T> where T : PlayerStateBase<T>
    {
        public Player Player { get; protected set; }
        public PlayerInputs Inputs { get; protected set; }
        public PlayerInfo Info { get; protected set; }

        public void Enter(Player player)
        {
            Player = player;
            Inputs = player.Inputs;
            Info = player.Info;
            OnEnter();
        }

        protected abstract void OnEnter();
        public abstract void OnExit();
        public abstract void OnUpdate();
        public abstract void OnLateUpdate();
        public abstract T GetNextState();
    }
}