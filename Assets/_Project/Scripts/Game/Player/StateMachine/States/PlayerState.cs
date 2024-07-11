namespace Fifbox.Game.Player.StateMachine.States
{
    public abstract class PlayerState : PlayerStateBase<PlayerState> { }

    public abstract class PlayerStateBase<T> where T : PlayerStateBase<T>
    {
        public Player Player { get; protected set; }

        public void Enter(Player player)
        {
            Player = player;
            OnEnter();
        }

        protected virtual void OnEnter() { }
        public virtual void OnExit() { }
        public virtual void OnUpdate() { }
        public virtual void OnLateUpdate() { }
        public abstract T GetNextState();
    }
}