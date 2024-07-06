using Fifbox.Content.Base;
using UnityEngine;

namespace Fifbox.Content.Player
{
    [CreateAssetMenu(fileName = "Player Config", menuName = "Fifbox/Configs/PlayerConfig", order = 0)]
    public class PlayerConfig : Config<PlayerConfig>
    {
        [Header("Body properties")]
        public float mass = 65f;
        public float width = 0.41f;
        public float fullHeight = 1.7f;
        public float crouchHeight = 1.15f;

        [Header("Physics properties")]
        public float gravityMultiplier = 1.275f;
        public float friction = 8f;

        [Header("Move properties")]
        public float walkSpeed = 2f;
        public float walkAcceleration = 12f;
        public float walkDeceleration = 2f;

        [Space(9)]

        public float runSpeed = 5.2f;
        public float runAcceleration = 8f;
        public float runDeceleration = 2.7f;

        [Space(9)]

        public float crouchSpeed = 1.2f;
        public float crouchAcceleration = 17f;
        public float crouchDeceleration = 2f;

        [Space(9)]

        public float maxSpeed = 24f;

        [Header("Jump properties")]
        public float walkJumpForce = 4f;
        public float runJumpForce = 4.9f;
        public float crouchJumpForce = 3.5f;

        [Space(9)]

        public float jumpBufferTime = 0.1f;

        [Header("Air handling")]
        public float airAcceleration = 28f;
        public float airSpeedCap = 3.1f;

        [Header("Noclip")]
        public float noclipNormalFlySpeed = 15f;
        public float noclipFastFlySpeed = 27.3f;
        public float noclipVerticalModifierSpeed = 15f;

        [Header("Ground handling")]
        public float maxStepHeight = 0.4f;
        public float stepDownBufferHeight = 0.5f;
    }
}