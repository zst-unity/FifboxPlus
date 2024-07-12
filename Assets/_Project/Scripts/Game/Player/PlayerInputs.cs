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

        public Vector3 OrientationEulerAngles
        {
            get => _orientationEulerAngles;
            set
            {
                _orientationEulerAngles = value;
                OnOrientationEulerAnglesChanged(_orientationEulerAngles);
            }
        }
        private Vector3 _orientationEulerAngles;

        public event Action<Vector3> OnOrientationEulerAnglesChanged = delegate { };
        public Action tryJump = delegate { };

        public PlayerInputsInfo GetInfo()
        {
            return new(_moveVector, _wantsToRun, _wantsToCrouch, _wantsToFlyFast, _wantsToAscend, _wantsToDescend, _nocliping);
        }
    }

    [Serializable]
    public readonly struct PlayerInputsInfo
    {
        public readonly Vector2 moveVector;
        public readonly bool wantsToRun;
        public readonly bool wantsToCrouch;
        public readonly bool wantsToFlyFast;
        public readonly bool wantsToAscend;
        public readonly bool wantsToDescend;
        public readonly bool nocliping;

        public PlayerInputsInfo(Vector2 moveVector, bool wantsToRun, bool wantsToCrouch, bool wantsToFlyFast, bool wantsToAscend, bool wantsToDescend, bool nocliping)
        {
            this.moveVector = moveVector;
            this.wantsToRun = wantsToRun;
            this.wantsToCrouch = wantsToCrouch;
            this.wantsToFlyFast = wantsToFlyFast;
            this.wantsToAscend = wantsToAscend;
            this.wantsToDescend = wantsToDescend;
            this.nocliping = nocliping;
        }
    }
}