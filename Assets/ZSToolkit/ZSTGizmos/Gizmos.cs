using UnityEditor;
using UnityEngine;

namespace ZSToolkit.ZSTGizmos
{
    public static class ZSTGizmos
    {
        public static void DrawWireCylinder(Vector3 position, Quaternion rotation, Vector3 scale, float radius, float height, Color color)
        {
            Handles.color = color;

            var space = Matrix4x4.TRS(position, rotation, scale);
            using (new Handles.DrawingScope(space))
            {
                var pointOffset = height / 2;

                //draw sideways
                Handles.DrawLine(new Vector3(0, pointOffset, -radius), new Vector3(0, -pointOffset, -radius));
                Handles.DrawLine(new Vector3(0, pointOffset, radius), new Vector3(0, -pointOffset, radius));
                //draw frontways
                Handles.DrawLine(new Vector3(-radius, pointOffset, 0), new Vector3(-radius, -pointOffset, 0));
                Handles.DrawLine(new Vector3(radius, pointOffset, 0), new Vector3(radius, -pointOffset, 0));
                //draw center
                Handles.DrawWireDisc(Vector3.up * pointOffset, Vector3.up, radius);
                Handles.DrawWireDisc(Vector3.down * pointOffset, Vector3.up, radius);
            }
        }
    }
}