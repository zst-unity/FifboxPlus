using Fifbox.Game.Player.StateMachine.States.OnGroundSubStates;
using UnityEngine;

namespace Fifbox.Game.Player.StateMachine.States
{
    public class OnGroundState : PlayerState
    {
        private float _targetDeceleration;

        public PlayerStateMachine<OnGroundSubState, IdlingState> StateMachine { get; private set; }

        protected override void OnEnter()
        {
            StateMachine = new(Player, PlayerInputs);
            StateMachine.Start();

            if (Player.Info.jumpBufferTimer > 0f)
            {
                TryJump();
                Player.Info.jumpBufferTimer = 0f;
            }

            PlayerInputs.tryJump += TryJump;
        }

        public override void OnExit()
        {
            StateMachine.Stop();
            PlayerInputs.tryJump -= TryJump;
        }

        private void TryJump()
        {
            if (Player.Info.ground.angle > Player.Config.slopeAngleLimit) return;

            var targetForce = StateMachine.CurrentState.JumpForce;
            Player.Rigidbody.linearVelocity = new(Player.Rigidbody.linearVelocity.x, targetForce, Player.Rigidbody.linearVelocity.z);
        }

        public override PlayerState GetNextState()
        {
            if (PlayerInputs.Nocliping) return new NoclipingState();

            if (!Player.Info.touchingGround) return new InAirState();
            else return null;
        }

        public override void OnUpdate()
        {
            StateMachine.Update();

            var shouldDisableFloatingCharacter = Player.Info.touchingCeiling || Player.Info.ground.angle > Player.Config.slopeAngleLimit;
            Player.Info.currentMaxStepHeight = shouldDisableFloatingCharacter ? 0f : Player.Config.maxStepHeight;
            Player.Info.currentStepDownBufferHeight = shouldDisableFloatingCharacter ? 0.02f : Player.Config.stepDownBufferHeight;
            Player.UpdateColliderAndCenter();

            var frictionMultiplierValue = Mathf.Clamp(Player.Info.ground.angle - Player.Config.slopeAngleLimit, 0f, 90f - Player.Config.slopeAngleLimit);
            var frictionMultiplier = 1f - Mathf.InverseLerp(0f, 90f - Player.Config.slopeAngleLimit, frictionMultiplierValue);
            ApplyFriction(frictionMultiplier);

            if (Player.Info.ground.angle > Player.Config.slopeAngleLimit)
            {
                Player.Rigidbody.linearVelocity += Player.Config.gravityMultiplier * Time.deltaTime * Physics.gravity;
            }

            Accelerate();
        }

        private void ApplyFriction(float frictionMultiplier)
        {
            float newSpeed, control;

            var speed = Player.Rigidbody.linearVelocity.magnitude;
            var drop = 0f;

            if (PlayerInputs.MoveVector.magnitude > 0f)
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
            var (_, wishSpeed, wishDir) = PlayerUtility.GetWishValues
            (
                Player.Info.flatOrientation.right,
                Player.Info.flatOrientation.forward,
                PlayerInputs.MoveVector,
                Player.Config.maxSpeed,
                targetSpeed
            );

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

        public override void OnLateUpdate()
        {
            StateMachine.LateUpdate();
            MoveToGround();
        }

        private void MoveToGround()
        {
            if (Player.Info.touchingCeiling || Player.Rigidbody.linearVelocity.y > 0.1f) return;

            var diff = Mathf.Abs(Player.transform.position.y - Player.Info.ground.height);
            if (Player.Info.ground.angle > Player.Config.slopeAngleLimit || diff > Player.Info.groundCheckSizeY) return;

            Player.transform.position = new(Player.transform.position.x, Player.Info.ground.height, Player.transform.position.z);
            Player.Rigidbody.linearVelocity = new(Player.Rigidbody.linearVelocity.x, 0f, Player.Rigidbody.linearVelocity.z);
        }
    }
}