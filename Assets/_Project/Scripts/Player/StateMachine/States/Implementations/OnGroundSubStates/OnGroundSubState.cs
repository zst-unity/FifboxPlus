namespace Fifbox.Player.StateMachine.States.OnGroundSubStates
{
    public abstract class OnGroundSubState : PlayerStateBase<OnGroundSubState, OnGroundSubStatesData>
    {
        public abstract float JumpForce { get; }
        public abstract float MoveSpeed { get; }
        public abstract float Acceleration { get; }
        public abstract float Deceleration { get; }
    }

    public partial class OnGroundSubStatesData : PlayerStatesDataBase { }
}