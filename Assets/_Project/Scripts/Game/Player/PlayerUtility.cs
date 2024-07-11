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

        public static (Vector3 wishVel, float wishSpeed, Vector2 wishDir) GetWishValues
        (
            PlayerOrientation flatOrientation,
            Vector2 moveVector,
            float maxSpeed,
            float wishVelMultiplier = 1f
        )
        {
            var wishVel = (flatOrientation.right * moveVector.x + flatOrientation.forward * moveVector.y) * wishVelMultiplier;
            var wishSpeed = wishVel.magnitude;
            var wishDir = new Vector2(wishVel.x, wishVel.z).normalized;

            if (wishSpeed != 0f && (wishSpeed > maxSpeed))
            {
                wishVel *= maxSpeed / wishSpeed;
                wishSpeed = maxSpeed;
            }

            return (wishVel, wishSpeed, wishDir);
        }

    }
}