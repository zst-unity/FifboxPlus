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
            PlayerHeights.CurrentMaxStepHeight = noclip ? 0f : Player.Config.maxStepHeight;
            Player.UpdateColliderAndCenter();
        }

        public override PlayerState GetNextState()
        {
            if (PlayerInputs.Nocliping) return null;

            if (Player.TouchingGround) return new OnGroundState();
            else return new InAirState();
        }

        public override void OnUpdate()
        {
            var targetSpeed = PlayerInputs.WantsToRun ? Player.Config.noclipFastFlySpeed : Player.Config.noclipNormalFlySpeed;

            var direction = PlayerInputs.FullOrientation.right * PlayerInputs.MoveVector.x + PlayerInputs.FullOrientation.forward * PlayerInputs.MoveVector.y;

            var verticalModifierDirection = 0f;
            if (PlayerInputs.WantsToCrouch) verticalModifierDirection -= 1f;
            if (PlayerInputs.WantsToAscend) verticalModifierDirection += 1f;
            var verticalModifierForce = verticalModifierDirection * Player.Config.noclipVerticalModifierSpeed;

            Player.Rigidbody.linearVelocity = (targetSpeed * direction) + Vector3.up * verticalModifierForce;
        }
    }
}