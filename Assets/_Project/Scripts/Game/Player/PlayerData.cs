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

        public float jumpBufferTimer;
        public Vector3 lastGroundedVelocity;
        public Vector3 fullOrientationEulerAngles;
        public int initialLayer;

        public float currentHeight;
        public float currentMaxStepHeight;
    }
}