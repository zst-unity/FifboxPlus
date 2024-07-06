using System;
using NaughtyAttributes;
using UnityEngine;

namespace Fifbox.FrontEnd.Player
{
    [Serializable]
    public class PlayerInputs
    {
        [ReadOnly, AllowNesting] public Vector2 moveVector;
        [ReadOnly, AllowNesting] public bool wantsToRun;
        [ReadOnly, AllowNesting] public bool wantsToCrouch;
        [ReadOnly, AllowNesting] public bool wantsToFlyFast;
        [ReadOnly, AllowNesting] public bool wantsToAscend;
        [ReadOnly, AllowNesting] public bool wantsToDescend;

        [ReadOnly, AllowNesting] public Vector3 orientationEulerAngles;

        public Action tryJump;
        public Action toggleNoclip;
    }
}