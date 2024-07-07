using UnityEngine;

namespace Fifbox.Game.Player
{
    public static class PlayerUtility
    {
        public static Vector3 GetColliderCenter(float maxStepHeight)
        {
            return new(0f, maxStepHeight / 2f, 0f);
        }

        public static Vector3 GetColliderSize(float width, float height, float maxStepHeight)
        {
            return new(width, height - maxStepHeight, width);
        }

        public static Vector3 GetCenterPosition(float height)
        {
            return new(0f, height / 2f, 0f);
        }
    }
}