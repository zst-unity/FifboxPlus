namespace Fifbox.Game.Player.StateMachine.States.OnGroundSubStates
{
    public abstract class OnGroundSubState : PlayerStateBase<OnGroundSubState>
    {
        public abstract float JumpForce { get; }
        public abstract float MoveSpeed { get; }
        public abstract float Acceleration { get; }
        public abstract float Deceleration { get; }
    }
}