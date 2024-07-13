using System;

namespace Fifbox.Player
{
    public class PlayerHeightsController
    {
        public PlayerHeightsInfo Info { get; private set; }

        public float CurrentHeight
        {
            get => _currentHeight;
            set
            {
                _currentHeight = value;
                Info = GetInfo();
            }
        }
        private float _currentHeight;

        public float CurrentMaxStepHeight
        {
            get => _currentMaxStepHeight;
            set
            {
                _currentMaxStepHeight = value;
                Info = GetInfo();
            }
        }
        private float _currentMaxStepHeight;

        public float CurrentStepDownBufferHeight
        {
            get => _currentStepDownBufferHeight;
            set
            {
                _currentStepDownBufferHeight = value;
                Info = GetInfo();
            }
        }
        private float _currentStepDownBufferHeight;

        public PlayerHeightsInfo GetInfo()
        {
            return new()
            {
                currentHeight = _currentHeight,
                currentMaxStepHeight = _currentMaxStepHeight,
                currentStepDownBufferHeight = _currentStepDownBufferHeight
            };
        }
    }

    [Serializable]
    public struct PlayerHeightsInfo
    {
        public float currentHeight;
        public float currentMaxStepHeight;
        public float currentStepDownBufferHeight;
    }
}