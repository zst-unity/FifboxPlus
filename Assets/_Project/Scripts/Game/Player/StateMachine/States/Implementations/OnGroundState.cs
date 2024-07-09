using System;
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

            Player.Data.currentMaxStepHeight = Player.Data.touchingCeiling ? 0f : Player.ConfigToUse.maxStepHeight;
            Player.UpdateColliderAndCenter();

            ApplyFriction();

            Player.Data.lastGroundedVelocity = Player.Rigidbody.linearVelocity;
            Player.Data.lastGroundedVelocity.y = 0f;

            Accelerate();
        }

        // TODO: вынести общее
        private void ApplyFriction()
        {
            float newSpeed, control;

            var speed = Player.Rigidbody.linearVelocity.magnitude;
            var drop = 0f;

            if (Player.Inputs.moveVector.magnitude > 0f)
            {
                _targetDeceleration = StateMachine.CurrentState.Deceleration;
            }

            control = speed < _targetDeceleration ? _targetDeceleration : speed;
            drop += control * Player.ConfigToUse.friction * Time.deltaTime;

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

            var wishVel = (Player.Orientation.right * Player.Inputs.moveVector.x + Player.Orientation.forward * Player.Inputs.moveVector.y) * targetSpeed;
            var wishSpeed = wishVel.magnitude;
            var wishDir = new Vector2(wishVel.x, wishVel.z).normalized;

            if (wishSpeed != 0f && (wishSpeed > Player.ConfigToUse.maxSpeed))
            {
                wishSpeed = Player.ConfigToUse.maxSpeed;
            }

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
            var targetForce = StateMachine.CurrentState.JumpForce;
            Player.Rigidbody.linearVelocity = new(Player.Rigidbody.linearVelocity.x, targetForce, Player.Rigidbody.linearVelocity.z);
        }
    }
}