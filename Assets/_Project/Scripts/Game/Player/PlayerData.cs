using System;
using UnityEngine;

namespace Fifbox.Game.Player
{
    [Serializable]
    public class PlayerData
    {
        public bool touchingGround;
        public bool touchingCeiling;
        public Vector3 groundNormal;
        public float groundAngle;
        public float groundHeight;
        public bool canStandUp;

        public bool crouching;
        public bool wasCrouchingLastFrame;
        public bool nocliping;

        public float currentHeight;
        public float currentMaxStepHeight;
        public float jumpBufferTimer;
        public Vector2 lastGroundedVelocity;
        public float lastMovingDeceleration;
        public Vector3 fullOrientationEulerAngles;
        public int initialLayer;
    }
}