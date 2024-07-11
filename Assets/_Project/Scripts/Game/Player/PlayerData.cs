using System;
using UnityEngine;

namespace Fifbox.Game.Player
{
    [Serializable]
    public class PlayerInputs
    {
        public Vector2 moveVector;
        public bool wantsToRun;
        public bool wantsToCrouch;
        public bool wantsToFlyFast;
        public bool wantsToAscend;
        public bool wantsToDescend;
        public bool nocliping;

        public Action<Vector3> setOrientationEulerAngles;
        public Action tryJump;
    }

    [Serializable]
    public class PlayerInfo
    {
        public PlayerMapInfo ground;
        public PlayerOrientation fullOrientation;
        public PlayerOrientation flatOrientation;

        public bool touchingGround;
        public bool touchingCeiling;

        public float jumpBufferTimer;
        public Vector3 lastGroundedVelocity;

        public float currentHeight;
        public float currentMaxStepHeight;
        public float currentStepDownBufferHeight;

        public float groundCheckSizeY;
    }

    [Serializable]
    public struct PlayerMapInfo
    {
        public readonly Vector3 normal;
        public readonly float angle;
        public readonly float height;

        public PlayerMapInfo(Vector3 normal, float angle, float height)
        {
            this.normal = normal;
            this.angle = angle;
            this.height = height;
        }
    }

    [Serializable]
    public struct PlayerOrientation
    {
        public readonly Vector3 eulerAngles;
        public readonly Quaternion quaternion;
        public readonly Vector3 forward;
        public readonly Vector3 right;
        public readonly Vector3 up;

        public PlayerOrientation(Vector3 eulerAngles)
        {
            this.eulerAngles = eulerAngles;
            quaternion = Quaternion.Euler(eulerAngles);
            forward = quaternion * Vector3.forward;
            right = quaternion * Vector3.right;
            up = quaternion * Vector3.up;
        }
    }
}