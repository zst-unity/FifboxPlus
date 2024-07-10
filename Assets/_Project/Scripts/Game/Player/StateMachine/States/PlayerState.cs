namespace Fifbox.Game.Player.StateMachine.States
{
    public abstract class PlayerState : PlayerStateBase<PlayerState>
    {
        public abstract void TryJump();
    }

    public abstract class PlayerStateBase<T> where T : PlayerStateBase<T>
    {
        public Player Player { get; protected set; }

        public abstract void Enter(Player player);
        public abstract void Exit();
        public abstract void Update();
        public abstract void LateUpdate();
        public abstract T GetNextState();
    }
}