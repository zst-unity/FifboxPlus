using Fifbox.Game.Player.StateMachine.States.OnGroundSubStates;
using UnityEngine;
using ZSToolkit.ZSTUtility.Extensions;

namespace Fifbox.Game.Player.StateMachine.States
{
    public class OnGroundState : PlayerState
    {
        private float _targetDeceleration;

        public PlayerStateMachine<OnGroundSubState, IdlingState, OnGroundSubStatesData> StateMachine { get; private set; }

        protected override void OnEnter()
        {
            StateMachine = new(Player, PlayerInputs, PlayerHeights);
            StateMachine.Start();

            Player.Rigidbody.SetVelocityY(0f);

            if (Data.jumpBufferTimer > 0f)
            {
                TryJump();
                Data.jumpBufferTimer = 0f;
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
            if (Player.GroundInfo.angle > Player.Config.slopeAngleLimit) return;

            var targetForce = StateMachine.CurrentState.JumpForce;
            Player.Rigidbody.SetVelocityY(targetForce);
        }

        public override PlayerState GetNextState()
        {
            if (PlayerInputs.Nocliping) return new NoclipingState();

            if (!Player.TouchingGround) return new InAirState();
            else return null;
        }

        public override void OnUpdate()
        {
            StateMachine.Update();

            var isOnSlopeLimit = Player.GroundInfo.angle > Player.Config.slopeAngleLimit;
            var shouldDisableFloatingCharacter = Player.TouchingCeiling || isOnSlopeLimit;
            PlayerHeights.CurrentMaxStepHeight = shouldDisableFloatingCharacter ? 0.05f : Player.Config.maxStepHeight;
            PlayerHeights.CurrentStepDownBufferHeight = Player.TouchingCeiling ? 0f : Player.Config.stepDownBufferHeight;
            Player.UpdateColliderAndCenter();

            if (isOnSlopeLimit) ApplyGravity();
            else ApplyFriction();

            Accelerate();
        }

        private void ApplyGravity()
        {
            Player.Rigidbody.linearVelocity += Player.Config.gravityMultiplier * Time.deltaTime * Physics.gravity;
        }

        private void ApplyFriction()
        {
            float newSpeed, control;

            var speed = Player.Rigidbody.linearVelocity.magnitude;
            var drop = 0f;

            if (PlayerInputs.MoveVector.magnitude > 0f)
            {
                _targetDeceleration = StateMachine.CurrentState.Deceleration;
            }

            control = speed < _targetDeceleration ? _targetDeceleration : speed;
            drop += control * Player.Config.friction * Time.deltaTime;

            newSpeed = Mathf.Max(speed - drop, 0f);

            if (newSpeed != speed)
            {
                newSpeed /= speed;
                Player.Rigidbody.linearVelocity *= newSpeed;
            }
        }

        private void Accelerate()
        {
            var targetSpeed = Player.GroundInfo.angle > Player.Config.slopeAngleLimit ? 1f : StateMachine.CurrentState.MoveSpeed;
            var (_, wishSpeed, wishDir) = PlayerUtility.GetWishValues
            (
                PlayerInputs.FlatOrientation.right,
                PlayerInputs.FlatOrientation.forward,
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
            if (Player.TouchingCeiling || Player.Rigidbody.linearVelocity.y > 0.1f) return;

            var diff = Mathf.Abs(Player.transform.position.y - Player.GroundInfo.height);
            if (Player.GroundInfo.angle > Player.Config.slopeAngleLimit || diff > Player.GroundCheckSizeY) return;

            Player.transform.position = new(Player.transform.position.x, Player.GroundInfo.height, Player.transform.position.z);
            Player.Rigidbody.SetVelocityY(0f);
        }
    }
}