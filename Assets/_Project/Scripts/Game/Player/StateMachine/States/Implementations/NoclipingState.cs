using Fifbox.ScriptableObjects;
using UnityEngine;
using ZSToolkit.ZSTUtility.Extensions;

namespace Fifbox.Game.Player.StateMachine.States
{
    public class NoclipingState : PlayerState
    {
        protected override void OnEnter()
        {
            SetNoclip(true);
        }

        public override void OnExit()
        {
            SetNoclip(false);
        }

        private void SetNoclip(bool noclip)
        {
            Player.gameObject.SetLayerForChildren(noclip ? FifboxLayers.NoclipingPlayerLayer.Index : Player.DefaultLayer);
            Player.Info.currentMaxStepHeight = noclip ? 0f : Player.Config.maxStepHeight;
            Player.UpdateColliderAndCenter();
        }

        public override PlayerState GetNextState()
        {
            if (Player.Inputs.nocliping) return null;

            if (Player.Info.touchingGround) return new OnGroundState();
            else return new InAirState();
        }

        public override void OnUpdate()
        {
            var targetSpeed = Player.Inputs.wantsToRun ? Player.Config.noclipFastFlySpeed : Player.Config.noclipNormalFlySpeed;

            var direction = Player.Info.fullOrientation.right * Player.Inputs.moveVector.x + Player.Info.fullOrientation.forward * Player.Inputs.moveVector.y;

            var verticalModifierDirection = 0f;
            if (Player.Inputs.wantsToCrouch) verticalModifierDirection -= 1f;
            if (Player.Inputs.wantsToAscend) verticalModifierDirection += 1f;
            var verticalModifierForce = verticalModifierDirection * Player.Config.noclipVerticalModifierSpeed;

            Player.Rigidbody.linearVelocity = (targetSpeed * direction) + Vector3.up * verticalModifierForce;
        }

        public override void TryJump() { }

        public override void OnLateUpdate()
        {

        }
    }
}