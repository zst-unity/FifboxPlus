using UnityEngine;

namespace ZSToolkit.ZSTUtility.StaticClasses
{
    public static class Keyboard
    {
        public static bool IsPressed(params KeyCode[] keys)
        {
            foreach (var key in keys)
            {
                if (Input.GetKeyDown(key)) return true;
            }

            return false;
        }

        public static bool IsReleased(params KeyCode[] keys)
        {
            foreach (var key in keys)
            {
                if (Input.GetKeyUp(key)) return true;
            }

            return false;
        }

        public static bool IsHolding(params KeyCode[] keys)
        {
            foreach (var key in keys)
            {
                if (Input.GetKey(key)) return true;
            }

            return false;
        }

        public static float AxisFrom(KeyCode negative, KeyCode positive)
        {
            float value = 0;

            if (IsHolding(negative) && value != -1) value--;
            if (IsHolding(positive) && value != 1) value++;

            return value;
        }
    }

    public static class Mouse
    {
        public static bool IsPressed(params MouseCode[] keys)
        {
            foreach (var key in keys)
            {
                if (Input.GetMouseButtonDown((int)key)) return true;
            }

            return false;
        }

        public static bool IsReleased(params MouseCode[] keys)
        {
            foreach (var key in keys)
            {
                if (Input.GetMouseButtonUp((int)key)) return true;
            }

            return false;
        }

        public static bool IsHolding(params MouseCode[] keys)
        {
            foreach (var key in keys)
            {
                if (Input.GetMouseButton((int)key)) return true;
            }

            return false;
        }
    }

    public enum MouseCode
    {
        Left = 0,
        Middle = 2,
        Right = 1
    }
}