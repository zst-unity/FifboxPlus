using Fifbox.API.ScriptableObjects.Configs;
using UnityEngine;

namespace Fifbox.Content.Player
{
    [CreateAssetMenu(fileName = "PlayerConfig", menuName = "Fifbox/Configs/Player Config", order = 0)]
    public class PlayerConfig : Config<PlayerConfig>
    {
        [Header("Body properties")]
        public float mass;
        public float width;
        public float fullHeight;
        public float crouchHeight;

        [Header("Physics properties")]
        public float gravityMultiplier;
        public float friction;

        [Header("Move properties")]
        public float walkSpeed;
        public float walkAcceleration;
        public float walkDeceleration;

        [Space(9)]

        public float runSpeed;
        public float runAcceleration;
        public float runDeceleration;

        [Space(9)]

        public float crouchSpeed;
        public float crouchAcceleration;
        public float crouchDeceleration;

        [Space(9)]

        public float maxSpeed;

        [Header("Jump properties")]
        public float walkJumpForce;
        public float runJumpForce;
        public float crouchJumpForce;

        [Space(9)]

        public float jumpBufferTime;

        [Header("Air handling")]
        public float airAcceleration;
        public float airSpeedCap;

        [Header("Noclip")]
        public float noclipNormalFlySpeed;
        public float noclipFastFlySpeed;
        public float noclipVerticalModifierSpeed;

        [Header("Ground handling")]
        public float maxStepHeight;
        public float stepDownBufferHeight;
    }
}