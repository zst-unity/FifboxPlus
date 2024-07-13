using UnityEngine;

namespace Fifbox.Player.StateMachine.States
{
    public class InAirState : PlayerState
    {
        protected override void OnEnter()
        {
            PlayerInputs.tryJump += TryJump;
        }

        public override void OnExit()
        {
            PlayerInputs.tryJump -= TryJump;
        }

        private void TryJump()
        {
            Data.jumpBufferTimer = Player.Config.jumpBufferTime;
        }

        public override PlayerState GetNextState()
        {
            if (PlayerInputs.Nocliping) return new NoclipingState();

            if (Player.TouchingGround) return new OnGroundState();
            else return null;
        }

        public override void OnUpdate()
        {
            if (Data.jumpBufferTimer > 0f) Data.jumpBufferTimer -= Time.deltaTime;

            PlayerHeights.CurrentMaxStepHeight = PlayerInputs.WantsToCrouch ?
                Player.Config.maxStepHeight / 2 + Player.Config.fullHeight - Player.Config.crouchHeight :
                Player.Config.maxStepHeight;

            Player.UpdateColliderAndCenter();

            Player.Rigidbody.linearVelocity += Player.Config.gravityMultiplier * Time.deltaTime * Physics.gravity;
            Accelerate();
        }

        private void Accelerate()
        {
            var (_, wishSpeed, wishDir) = PlayerUtility.GetWishValues
            (
                PlayerInputs.FlatOrientation.right,
                PlayerInputs.FlatOrientation.forward,
                PlayerInputs.MoveVector,
                Player.Config.maxSpeed,
                2f
            );

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
    }
}