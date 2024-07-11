using System;
using UnityEngine;

namespace Fifbox.Game.Player
{
    public class PlayerInputs
    {
        public PlayerInputsInfo Info { get; private set; }

        public Vector2 MoveVector
        {
            get => _moveVector;
            set
            {
                _moveVector = value;
                Info = GetInfo();
            }
        }
        private Vector2 _moveVector;

        public bool WantsToRun
        {
            get => _wantsToRun;
            set
            {
                _wantsToRun = value;
                Info = GetInfo();
            }
        }
        private bool _wantsToRun;

        public bool WantsToCrouch
        {
            get => _wantsToCrouch;
            set
            {
                _wantsToCrouch = value;
                Info = GetInfo();
            }
        }
        private bool _wantsToCrouch;

        public bool WantsToFlyFast
        {
            get => _wantsToFlyFast;
            set
            {
                _wantsToFlyFast = value;
                Info = GetInfo();
            }
        }
        private bool _wantsToFlyFast;

        public bool WantsToAscend
        {
            get => _wantsToAscend;
            set
            {
                _wantsToAscend = value;
                Info = GetInfo();
            }
        }
        private bool _wantsToAscend;

        public bool WantsToDescend
        {
            get => _wantsToDescend;
            set
            {
                _wantsToDescend = value;
                Info = GetInfo();
            }
        }
        private bool _wantsToDescend;

        public bool Nocliping
        {
            get => _nocliping;
            set
            {
                _nocliping = value;
                Info = GetInfo();
            }
        }
        private bool _nocliping;

        public Action<Vector3> setOrientationEulerAngles = delegate { };
        public Action tryJump = delegate { };

        public PlayerInputsInfo GetInfo()
        {
            return new PlayerInputsInfo
            {
                moveVector = _moveVector,
                wantsToRun = _wantsToRun,
                wantsToCrouch = _wantsToCrouch,
                wantsToFlyFast = _wantsToFlyFast,
                wantsToAscend = _wantsToAscend,
                wantsToDescend = _wantsToDescend,
                nocliping = _nocliping
            };
        }
    }

    [Serializable]
    public struct PlayerInputsInfo
    {
        public Vector2 moveVector;
        public bool wantsToRun;
        public bool wantsToCrouch;
        public bool wantsToFlyFast;
        public bool wantsToAscend;
        public bool wantsToDescend;
        public bool nocliping;
    }
}