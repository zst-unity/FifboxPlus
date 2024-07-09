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
                Player.ConfigToUse.maxStepHeight / 2 + Player.ConfigToUse.fullHeight - Player.ConfigToUse.crouchHeight :
                Player.ConfigToUse.maxStepHeight;

            Player.UpdateColliderAndCenter();

            ApplyGravity();
            Accelerate();
        }

        private void ApplyGravity()
        {
            Player.Rigidbody.linearVelocity += Player.ConfigToUse.gravityMultiplier * Time.deltaTime * Physics.gravity;
        }

        private void Accelerate()
        {
            var targetSpeed = Player.Data.lastGroundedVelocity.magnitude;
            var wishVel = (Player.Orientation.right * Player.Inputs.moveVector.x + Player.Orientation.forward * Player.Inputs.moveVector.y) * targetSpeed;
            var wishSpeed = wishVel.magnitude;
            var wishDir = new Vector2(wishVel.x, wishVel.z).normalized;

            if (wishSpeed != 0f && (wishSpeed > Player.ConfigToUse.maxSpeed))
            {
                wishSpeed = Player.ConfigToUse.maxSpeed;
            }

            var velocity = new Vector2(Player.Rigidbody.linearVelocity.x, Player.Rigidbody.linearVelocity.z);
            var currentSpeed = Vector2.Dot(velocity, wishDir);

            var airWishSpeed = Mathf.Min(wishSpeed, Player.ConfigToUse.airSpeedCap);
            var addSpeed = airWishSpeed - currentSpeed;

            if (addSpeed <= 0) return;

            var accelSpeed = Player.ConfigToUse.airAcceleration * Time.deltaTime * wishSpeed;
            accelSpeed = Mathf.Min(accelSpeed, addSpeed);

            velocity += wishDir * accelSpeed;
            Player.Rigidbody.linearVelocity = new(velocity.x, Player.Rigidbody.linearVelocity.y, velocity.y);
        }

        public override void TryJump()
        {
            Player.Data.jumpBufferTimer = Player.ConfigToUse.jumpBufferTime;
        }
    }
}