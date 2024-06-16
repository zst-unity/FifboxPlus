using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ZSToolkit.ZSTUtility.Extensions
{
    public static class ZSTColorExtensions
    {
        public static string ToHex(this Color32 color, bool startsWithHash = true, bool includeAlpha = false)
        {
            var hex = includeAlpha ?
                string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", color.r, color.g, color.b, color.a) :
                string.Format("{0:X2}{1:X2}{2:X2}", color.r, color.g, color.b);

            return startsWithHash ? $"#{hex}" : hex;
        }

        public static string ToHex(this Color color, bool startsWithHash = true, bool includeAlpha = false)
        {
            Color32 color32 = color;
            return color32.ToHex(startsWithHash, includeAlpha);
        }

        public static Color ToColor(this string hex)
        {
            if (hex.StartsWith("#")) hex = hex[1..];

            var hexColor = Convert.ToInt32(hex, 16);
            if (hex.Length == 8)
            {
                var red = (byte)((hexColor >> 24) & 0xFF);
                var green = (byte)((hexColor >> 16) & 0xFF);
                var blue = (byte)((hexColor >> 8) & 0xFF);
                var alpha = (byte)(hexColor & 0xFF);

                return new Color32(red, green, blue, alpha);
            }
            else if (hex.Length == 6)
            {
                var red = (byte)((hexColor >> 16) & 0xFF);
                var green = (byte)((hexColor >> 8) & 0xFF);
                var blue = (byte)(hexColor & 0xFF);

                return new Color32(red, green, blue, 255);
            }
            else throw new ArgumentException("Invalid hex string");
        }
    }

    public static class ZSTArrayExtensions
    {
        public static T GetRandomValue<T>(this IEnumerable<T> collection)
        {
            return collection.ElementAt(UnityEngine.Random.Range(0, collection.Count()));
        }

        public static T GetNext<T>(this IEnumerable<T> collection, int current)
        {
            return current == collection.Count() - 1 ? collection.First() : collection.ElementAt(current + 1);
        }

        public static T GetNext<T>(this IEnumerable<T> collection, T current)
        {
            return current.Equals(collection.Last()) ? collection.First() : collection.ElementAt(collection.IndexOf(current) + 1);
        }

        public static T GetPrevious<T>(this IEnumerable<T> collection, int current)
        {
            return current == 0 ? collection.Last() : collection.ElementAt(current - 1);
        }

        public static T GetPrevious<T>(this IEnumerable<T> collection, T current)
        {
            return current.Equals(collection.First()) ? collection.Last() : collection.ElementAt(collection.IndexOf(current) - 1);
        }

        public static int IndexOf<T>(this IEnumerable<T> source, T value)
        {
            int index = 0;
            var comparer = EqualityComparer<T>.Default;
            foreach (T item in source)
            {
                if (comparer.Equals(item, value)) return index;
                index++;
            }
            return -1;
        }
    }

    public static class ZSTGameObjectExtensions
    {
        public static GameObject Spawn(this GameObject gameObject, Vector3? position = null, Quaternion? rotation = null, Transform parent = null)
        {
            var obj = Object.Instantiate(gameObject);
            if (parent) obj.transform.SetParent(parent);
            if (position != null) obj.transform.localPosition = position.Value;
            if (rotation != null) obj.transform.localRotation = rotation.Value;

            return obj;
        }
    }

    public static class ZSTMath
    {
        public static float Remap(this float value, float fromMin, float fromMax, float toMin, float toMax)
        {
            var fromAbs = value - fromMin;
            var fromMaxAbs = fromMax - fromMin;
            var normal = fromAbs / fromMaxAbs;
            var toMaxAbs = toMax - toMin;
            var toAbs = toMaxAbs * normal;
            return toAbs + toMin;
        }

        public static Vector2 RadianToVector2(float radian)
        {
            return new Vector2(Mathf.Cos(radian), Mathf.Sin(radian));
        }

        public static Vector2 DegreeToVector2(float degree)
        {
            return RadianToVector2(degree * Mathf.Deg2Rad);
        }

        public static Vector2 Abs(this Vector2 target)
        {
            return new Vector2
            (
                Mathf.Abs(target.x),
                Mathf.Abs(target.y)
            );
        }

        public static Vector2Int Abs(this Vector2Int target)
        {
            return new Vector2Int
            (
                Mathf.Abs(target.x),
                Mathf.Abs(target.y)
            );
        }

        public static Vector2 ClampX(this Vector2 target, float min, float max)
        {
            return new Vector2
            (
                Mathf.Clamp(target.x, min, max),
                target.y
            );
        }

        public static Vector2 ClampY(this Vector2 target, float min, float max)
        {
            return new Vector2
            (
                target.x,
                Mathf.Clamp(target.y, min, max)
            );
        }

        public static Vector2 Clamp(this Vector2 target, Vector2 min, Vector2 max)
        {
            return new Vector2
            (
                Mathf.Clamp(target.x, min.x, max.x),
                Mathf.Clamp(target.y, min.y, max.y)
            );
        }

        public static Vector2 Round(this Vector2 target, float roundTo = 1f)
        {
            return new Vector2
            (
                Mathf.Round(target.x / roundTo) * roundTo,
                Mathf.Round(target.y / roundTo) * roundTo
            );
        }

        public static Vector2Int RoundToInt(this Vector2 target)
        {
            return new Vector2Int
            (
                Mathf.RoundToInt(target.x),
                Mathf.RoundToInt(target.y)
            );
        }

        public static Vector2 Floor(this Vector2 target, float roundTo = 1f)
        {
            return new Vector2
            (
                Mathf.Floor(target.x / roundTo) * roundTo,
                Mathf.Floor(target.y / roundTo) * roundTo
            );
        }

        public static Vector2Int FloorToInt(this Vector2 target)
        {
            return new Vector2Int
            (
                Mathf.FloorToInt(target.x),
                Mathf.FloorToInt(target.y)
            );
        }

        public static Vector2 ProjectOnLine(Vector2 vector, Vector2 normal)
        {
            return Vector3.ProjectOnPlane(vector, normal);
        }

        public static Vector3 Round(this Vector3 target, float roundTo = 1f)
        {
            return new Vector3
            (
                Mathf.Round(target.x / roundTo) * roundTo,
                Mathf.Round(target.y / roundTo) * roundTo,
                Mathf.Round(target.z / roundTo) * roundTo
            );
        }

        public static Vector3Int RoundToInt(this Vector3 target)
        {
            return new Vector3Int
            (
                Mathf.RoundToInt(target.x),
                Mathf.RoundToInt(target.y),
                Mathf.RoundToInt(target.z)
            );
        }

        public static Vector3 Floor(this Vector3 target, float floorTo = 1f)
        {
            return new Vector3
            (
                Mathf.Floor(target.x / floorTo) * floorTo,
                Mathf.Floor(target.y / floorTo) * floorTo,
                Mathf.Floor(target.z / floorTo) * floorTo
            );
        }

        public static Vector3Int FloorToInt(this Vector3 target)
        {
            return new Vector3Int
            (
                Mathf.FloorToInt(target.x),
                Mathf.FloorToInt(target.y),
                Mathf.FloorToInt(target.z)
            );
        }

        public static Vector3 Clamp(this Vector3 target, Vector3 min, Vector3 max)
        {
            return new Vector3
            (
                Mathf.Clamp(target.x, min.x, max.x),
                Mathf.Clamp(target.y, min.y, max.y),
                Mathf.Clamp(target.z, min.z, max.z)
            );
        }

        public static Vector3 ClampMinimum(this Vector3 target, Vector3 min)
        {
            return new Vector3
            (
                target.x < min.x ? min.x : target.x,
                target.y < min.y ? min.y : target.y,
                target.z < min.z ? min.z : target.z
            );
        }

        public static Vector3 ClampMinimum(this Vector3 target, float x, float y, float z)
        {
            return new Vector3
            (
                target.x < x ? x : target.x,
                target.y < y ? y : target.y,
                target.z < z ? z : target.z
            );
        }

        public static Vector3 ClampMaximum(this Vector3 target, Vector3 max)
        {
            return new Vector3
            (
                target.x > max.x ? max.x : target.x,
                target.y > max.y ? max.y : target.y,
                target.z > max.z ? max.z : target.z
            );
        }

        public static Vector3 ClampMaximum(this Vector3 target, float x, float y, float z)
        {
            return new Vector3
            (
                target.x > x ? x : target.x,
                target.y > y ? y : target.y,
                target.z > z ? z : target.z
            );
        }

        public static Vector3 Abs(this Vector3 target)
        {
            return new Vector3
            (
                Mathf.Abs(target.x),
                Mathf.Abs(target.y),
                Mathf.Abs(target.z)
            );
        }

        public static Vector3Int Abs(this Vector3Int target)
        {
            return new Vector3Int
            (
                Mathf.Abs(target.x),
                Mathf.Abs(target.y),
                Mathf.Abs(target.z)
            );
        }

        public static Vector3 Multiply(this Vector3 target, Vector3 multiplier)
        {
            return new Vector3
            (
                target.x * multiplier.x,
                target.y * multiplier.y,
                target.z * multiplier.z
            );
        }

        public static Vector3 Multiply(this Vector3 target, float x, float y, float z)
        {
            return new Vector3
            (
                target.x * x,
                target.y * y,
                target.z * z
            );
        }

        public static Vector3 Divide(this Vector3 target, Vector3 divider)
        {
            return new Vector3
            (
                target.x / divider.x,
                target.y / divider.y,
                target.z / divider.z
            );
        }

        public static Vector3 Divide(this Vector3 target, float x, float y, float z)
        {
            return new Vector3
            (
                target.x / x,
                target.y / y,
                target.z / z
            );
        }
    }
}