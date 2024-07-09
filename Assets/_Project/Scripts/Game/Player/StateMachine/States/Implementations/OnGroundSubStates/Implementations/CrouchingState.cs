using Fifbox.ScriptableObjects;
using UnityEngine;

namespace Fifbox.Game.Player.StateMachine.States.OnGroundSubStates
{
    public class CrouchingState : OnGroundSubState
    {
        private bool _canStandUp;

        public override float JumpForce => _canStandUp ? Player.ConfigToUse.crouchJumpForce : 0f;
        public override float MoveSpeed => Player.ConfigToUse.crouchSpeed;
        public override float Acceleration => Player.ConfigToUse.crouchAcceleration;
        public override float Deceleration => Player.ConfigToUse.crouchDeceleration;

        private void SetCrouching(bool crouching)
        {
            Player.Data.currentHeight = crouching ? Player.ConfigToUse.crouchHeight : Player.ConfigToUse.fullHeight;
            Player.UpdateColliderAndCenter();
        }

        public override void Enter(Player player)
        {
            Player = player;

            SetCrouching(true);
        }

        public override void Exit()
        {
            SetCrouching(false);
        }

        public override OnGroundSubState GetNextState()
        {
            if (Player.Inputs.wantsToCrouch || !_canStandUp) return null;

            if (Player.Inputs.moveVector.magnitude > 0f)
            {
                if (Player.Inputs.wantsToRun) return new RunningState();
                else return new WalkingState();
            }

            return new IdlingState();
        }

        public override void Update()
        {
            var canStandUpCheckSize = new Vector3(Player.WidthForChecking, Player.ConfigToUse.fullHeight - Player.ConfigToUse.crouchHeight, Player.WidthForChecking);
            var canStandUpCheckPosition = Player.transform.position + Vector3.up * (Player.ConfigToUse.fullHeight + Player.ConfigToUse.crouchHeight) / 2;
            _canStandUp = !Physics.CheckBox(canStandUpCheckPosition, canStandUpCheckSize / 2f, Quaternion.identity, FifboxLayers.GroundLayers);
        }
    }
}