using Fifbox.ScriptableObjects;
using UnityEngine;
using ZSToolkit.ZSTUtility.Extensions;

namespace Fifbox.Game.Player.StateMachine.States
{
    public class NoclipingState : PlayerState
    {
        public override void Enter(Player player)
        {
            Player = player;
            SetNoclip(true);
        }

        public override void Exit()
        {
            SetNoclip(false);
        }

        private void SetNoclip(bool noclip)
        {
            Player.gameObject.SetLayerForChildren(noclip ? FifboxLayers.NoclipingPlayerLayer.Index : Player.Data.initialLayer);
            Player.Data.currentMaxStepHeight = noclip ? 0f : Player.ConfigToUse.maxStepHeight;
            Player.UpdateColliderAndCenter();
        }

        public override PlayerState GetNextState()
        {
            if (Player.Inputs.nocliping) return null;

            if (Player.Data.touchingGround) return new OnGroundState();
            else return new InAirState();
        }

        public override void Update()
        {
            var targetSpeed = Player.Inputs.wantsToRun ? Player.ConfigToUse.noclipFastFlySpeed : Player.ConfigToUse.noclipNormalFlySpeed;

            var fullOrientation = Quaternion.Euler(Player.Data.fullOrientationEulerAngles.x, Player.Data.fullOrientationEulerAngles.y, 0f);
            var forward = fullOrientation * Vector3.forward;
            var right = fullOrientation * Vector3.right;
            var direction = right * Player.Inputs.moveVector.x + forward * Player.Inputs.moveVector.y;

            var verticalModifierDirection = 0f;
            if (Player.Inputs.wantsToCrouch) verticalModifierDirection -= 1f;
            if (Player.Inputs.wantsToAscend) verticalModifierDirection += 1f;
            var verticalModifierForce = verticalModifierDirection * Player.ConfigToUse.noclipVerticalModifierSpeed;

            Player.Rigidbody.linearVelocity = (targetSpeed * direction) + Vector3.up * verticalModifierForce;
        }

        public override void TryJump() { }
    }
}