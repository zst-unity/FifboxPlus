using UnityEngine;

namespace Fifbox.Game.Player.StateMachine.States
{
    public class InAirState : PlayerState
    {
        protected override void OnEnter()
        {

        }

        public override void OnExit()
        {

        }

        public override PlayerState GetNextState()
        {
            if (Player.Inputs.nocliping) return new NoclipingState();

            if (Player.Info.touchingGround) return new OnGroundState();
            else return null;
        }

        public override void OnUpdate()
        {
            if (Player.Info.jumpBufferTimer > 0f) Player.Info.jumpBufferTimer -= Time.deltaTime;

            Player.Info.currentMaxStepHeight = Player.Inputs.wantsToCrouch ?
                Player.Config.maxStepHeight / 2 + Player.Config.fullHeight - Player.Config.crouchHeight :
                Player.Config.maxStepHeight;

            Player.UpdateColliderAndCenter();

            Player.Rigidbody.linearVelocity += Player.Config.gravityMultiplier * Time.deltaTime * Physics.gravity;
            Accelerate();
        }

        private void Accelerate()
        {
            var targetSpeed = Player.Info.lastGroundedVelocity.magnitude;
            var (_, wishSpeed, wishDir) = Player.GetWishValues(targetSpeed);

            var velocity = new Vector2(Player.Rigidbody.linearVelocity.x, Player.Rigidbody.linearVelocity.z);
            var currentSpeed = Vector2.Dot(velocity, wishDir);

            var airWishSpeed = Mathf.Min(wishSpeed, Player.Config.airSpeedCap);
            var addSpeed = airWishSpeed - currentSpeed;

            if (addSpeed <= 0) return;

            var accelSpeed = Player.Config.airAcceleration * Time.deltaTime * wishSpeed;
            accelSpeed = Mathf.Min(accelSpeed, addSpeed);

            velocity += wishDir * accelSpeed;
            Player.Rigidbody.linearVelocity = new(velocity.x, Player.Rigidbody.linearVelocity.y, velocity.y);
        }

        public override void TryJump()
        {
            Player.Info.jumpBufferTimer = Player.Config.jumpBufferTime;
        }

        public override void OnLateUpdate()
        {

        }
    }
}