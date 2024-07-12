using System;
using UnityEngine;

namespace Fifbox.Game.Player
{
    [Serializable]
    public class PlayerInfo
    {
        public float jumpBufferTimer;

        public float currentHeight;
        public float currentMaxStepHeight;
        public float currentStepDownBufferHeight;

        public float groundCheckSizeY;
    }
}