using UnityEngine;

namespace Fifbox.Game.Player.StateMachine.States
{
    public class InAirState : PlayerState
    {
        public override void Enter(Player player)
        {
            Player = player;
        }

        public override void Exit()
        {

        }

        public override PlayerState GetNextState()
        {
            if (Player.Inputs.nocliping) return new NoclipingState();

            if (Player.Data.touchingGround) return new OnGroundState();
            else return null;
        }

        public override void Update()
        {
            if (Player.Data.jumpBufferTimer > 0f) Player.Data.jumpBufferTimer -= Time.deltaTime;

            Player.Data.currentMaxStepHeight = Player.Inputs.wantsToCrouch ?
                Player.Config.maxStepHeight / 2 + Player.Config.fullHeight - Player.Config.crouchHeight :
                Player.Config.maxStepHeight;

            Player.UpdateColliderAndCenter();

            Player.ApplyGravity();
            Accelerate();
        }

        private void Accelerate()
        {
            var targetSpeed = Player.Data.lastGroundedVelocity.magnitude;
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
            Player.Data.jumpBufferTimer = Player.Config.jumpBufferTime;
        }

        public override void LateUpdate()
        {

        }
    }
}