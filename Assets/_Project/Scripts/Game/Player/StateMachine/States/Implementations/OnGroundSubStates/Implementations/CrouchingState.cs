using Fifbox.ScriptableObjects;
using UnityEngine;

namespace Fifbox.Game.Player.StateMachine.States.OnGroundSubStates
{
    public class CrouchingState : OnGroundSubState
    {
        private bool _canStandUp;

        public override float JumpForce => _canStandUp ? Player.Config.crouchJumpForce : 0f;
        public override float MoveSpeed => Player.Config.crouchSpeed;
        public override float Acceleration => Player.Config.crouchAcceleration;
        public override float Deceleration => Player.Config.crouchDeceleration;

        private void SetCrouching(bool crouching)
        {
            Player.Info.currentHeight = crouching ? Player.Config.crouchHeight : Player.Config.fullHeight;
            Player.UpdateColliderAndCenter();
        }

        protected override void OnEnter()
        {
            SetCrouching(true);
        }

        public override void OnExit()
        {
            SetCrouching(false);
        }

        public override OnGroundSubState GetNextState()
        {
            if (PlayerInputs.WantsToCrouch || !_canStandUp) return null;

            if (PlayerInputs.MoveVector.magnitude > 0f)
            {
                if (PlayerInputs.WantsToRun) return new RunningState();
                else return new WalkingState();
            }

            return new IdlingState();
        }

        public override void OnUpdate()
        {
            var canStandUpCheckSize = new Vector3(Player.WidthForChecking, Player.Config.fullHeight - Player.Config.crouchHeight, Player.WidthForChecking);
            var canStandUpCheckPosition = Player.transform.position + Vector3.up * (Player.Config.fullHeight + Player.Config.crouchHeight) / 2;
            _canStandUp = !Physics.CheckBox(canStandUpCheckPosition, canStandUpCheckSize / 2f, Quaternion.identity, FifboxLayers.GroundLayers);
        }
    }
}