namespace Fifbox.Game.Player.StateMachine.States
{
    public abstract class PlayerState : PlayerStateBase<PlayerState> { }

    public abstract class PlayerStateBase<T> where T : PlayerStateBase<T>
    {
        public abstract void Enter(Player player);
        public abstract void Exit();
        public abstract void Update();
        public abstract T GetNextState();
    }
}