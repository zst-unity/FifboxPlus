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

        public Vector3 orientationEulerAngles;

        public Action tryJump;
        public Action toggleNoclip;
    }
}