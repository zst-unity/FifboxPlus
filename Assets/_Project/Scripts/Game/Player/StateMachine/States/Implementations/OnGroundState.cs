using Fifbox.Game.Player.StateMachine.States.OnGroundSubStates;
using UnityEngine;

namespace Fifbox.Game.Player.StateMachine.States
{
    public class OnGroundState : PlayerState
    {
        private float _targetDeceleration;

        public PlayerStateMachine<OnGroundSubState, IdlingState> StateMachine { get; private set; }

        public override void Enter(Player player)
        {
            Player = player;

            StateMachine = new(player);
            StateMachine.Start();

            if (Player.Data.jumpBufferTimer > 0f)
            {
                TryJump();
                Player.Data.jumpBufferTimer = 0f;
            }
        }

        public override void Exit()
        {
            StateMachine.Stop();
        }

        public override PlayerState GetNextState()
        {
            if (Player.Inputs.nocliping) return new NoclipingState();

            if (!Player.Data.touchingGround) return new InAirState();
            else return null;
        }

        public override void Update()
        {
            StateMachine.Update();

            var shouldDisableFloatingCharacter = Player.Data.touchingCeiling || Player.Data.groundAngle > Player.Config.slopeAngleLimit;
            Player.Data.currentMaxStepHeight = shouldDisableFloatingCharacter ? 0f : Player.Config.maxStepHeight;
            Player.UpdateColliderAndCenter();

            var ddd = Mathf.Clamp(Player.Data.groundAngle - Player.Config.slopeAngleLimit, 0f, 90f - Player.Config.slopeAngleLimit);
            var sds = 1f - Mathf.InverseLerp(0f, 90f - Player.Config.slopeAngleLimit, ddd);
            ApplyFriction(sds);

            if (Player.Data.groundAngle > Player.Config.slopeAngleLimit)
            {
                Player.ApplyGravity();
            }

            Player.Data.lastGroundedVelocity = Player.Rigidbody.linearVelocity;
            Player.Data.lastGroundedVelocity.y = 0f;

            Accelerate();
        }

        private void ApplyFriction(float frictionMultiplier)
        {
            float newSpeed, control;

            var speed = Player.Rigidbody.linearVelocity.magnitude;
            var drop = 0f;

            if (Player.Inputs.moveVector.magnitude > 0f)
            {
                _targetDeceleration = StateMachine.CurrentState.Deceleration;
            }

            control = speed < _targetDeceleration ? _targetDeceleration : speed;
            drop += control * Player.Config.friction * frictionMultiplier * Time.deltaTime;

            newSpeed = Mathf.Max(speed - drop, 0f);

            if (newSpeed != speed)
            {
                newSpeed /= speed;
                Player.Rigidbody.linearVelocity *= newSpeed;
            }
        }

        private void Accelerate()
        {
            var targetSpeed = StateMachine.CurrentState.MoveSpeed;
            var (_, wishSpeed, wishDir) = Player.GetWishValues(targetSpeed);

            var velocity = new Vector2(Player.Rigidbody.linearVelocity.x, Player.Rigidbody.linearVelocity.z);
            var currentSpeed = Vector2.Dot(velocity, wishDir);

            var addSpeed = wishSpeed - currentSpeed;
            if (addSpeed <= 0) return;

            var acceleration = StateMachine.CurrentState.Acceleration;
            var accelSpeed = acceleration * Time.deltaTime * wishSpeed;
            accelSpeed = Mathf.Min(accelSpeed, addSpeed);

            velocity += wishDir * accelSpeed;
            Player.Rigidbody.linearVelocity = new(velocity.x, Player.Rigidbody.linearVelocity.y, velocity.y);
        }

        public override void TryJump()
        {
            if (Player.Data.groundAngle > Player.Config.slopeAngleLimit) return;

            var targetForce = StateMachine.CurrentState.JumpForce;
            Player.Rigidbody.linearVelocity = new(Player.Rigidbody.linearVelocity.x, targetForce, Player.Rigidbody.linearVelocity.z);
        }

        public override void LateUpdate()
        {
            MoveToGround();
        }

        private void MoveToGround()
        {
            if (Player.Data.touchingCeiling || Player.Rigidbody.linearVelocity.y > 0f) return;

            var diff = Mathf.Abs(Player.transform.position.y - Player.Data.groundHeight);
            if (Player.Data.groundAngle > Player.Config.slopeAngleLimit || diff > Player.Data.groundCheckSizeY) return;

            Player.transform.position = new(Player.transform.position.x, Player.Data.groundHeight, Player.transform.position.z);
            Player.Rigidbody.linearVelocity = new(Player.Rigidbody.linearVelocity.x, 0f, Player.Rigidbody.linearVelocity.z);
        }
    }
}