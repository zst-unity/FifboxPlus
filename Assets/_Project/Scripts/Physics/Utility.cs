using System.Collections.Generic;
using UnityEngine;

namespace Fifbox.Physics
{
    public static class FifboxPhysicsUtility
    {
        public static Vector3[] GetWorldVertices(this BoxCollider collider)
        {
            var colliderTransform = collider.transform;
            var vertices = new Vector3[8];
            vertices[0] = colliderTransform.TransformPoint(collider.center + collider.size / 2);
            vertices[1] = colliderTransform.TransformPoint(collider.center + new Vector3(-collider.size.x, collider.size.y, collider.size.z) / 2);
            vertices[2] = colliderTransform.TransformPoint(collider.center + new Vector3(collider.size.x, collider.size.y, -collider.size.z) / 2);
            vertices[3] = colliderTransform.TransformPoint(collider.center + new Vector3(-collider.size.x, collider.size.y, -collider.size.z) / 2);
            vertices[4] = colliderTransform.TransformPoint(collider.center + new Vector3(collider.size.x, -collider.size.y, collider.size.z) / 2);
            vertices[5] = colliderTransform.TransformPoint(collider.center + new Vector3(-collider.size.x, -collider.size.y, collider.size.z) / 2);
            vertices[6] = colliderTransform.TransformPoint(collider.center + new Vector3(collider.size.x, -collider.size.y, -collider.size.z) / 2);
            vertices[7] = colliderTransform.TransformPoint(collider.center - collider.size / 2);

            return vertices;
        }

        public static (Vector3 a, Vector3 b)[] GetWorldEdges(this BoxCollider collider)
        {
            var vertices = collider.GetWorldVertices();
            var edges = new (Vector3 a, Vector3 b)[12];
            edges[0] = (vertices[0], vertices[1]);
            edges[1] = (vertices[0], vertices[2]);
            edges[2] = (vertices[0], vertices[4]);
            edges[3] = (vertices[1], vertices[3]);
            edges[4] = (vertices[1], vertices[5]);
            edges[5] = (vertices[2], vertices[3]);
            edges[6] = (vertices[2], vertices[6]);
            edges[7] = (vertices[3], vertices[7]);
            edges[8] = (vertices[4], vertices[6]);
            edges[9] = (vertices[4], vertices[5]);
            edges[10] = (vertices[5], vertices[7]);
            edges[11] = (vertices[6], vertices[7]);

            return edges;
        }

        public static Vector2[] GetEllipseLineIntersections(Vector2 ia, Vector2 ib, Vector2 c, Vector2 r)
        {
            var intersections = new List<Vector2>();

            var a = ia - c;
            var b = ib - c;

            if (a.x == b.x)
            {
                var y = r.y / r.x * Mathf.Sqrt(r.x * r.x - a.x * a.x);

                if (Mathf.Min(a.y, b.y) <= y && y <= Mathf.Max(a.y, b.y))
                {
                    intersections.Add(new(a.x + c.x, y + c.y));
                }

                if (Mathf.Min(a.y, b.y) <= -y && -y <= Mathf.Max(a.y, b.y))
                {
                    intersections.Add(new(a.x + c.x, -y + c.y));
                }
            }
            else
            {
                var s1 = (b.y - a.y) / (b.x - b.y);
                var s2 = a.y - s1 * a.x;

                var eqA = s1 * s1 * r.x * r.x + r.y * r.y;
                var eqB = 2 * s1 * s2 * r.x * r.x;
                var eqC = r.x * r.x * s2 * s2 - r.x * r.x * r.y * r.y;

                var d = eqB * eqB - 4 * eqA * eqC;

                if (d > 0)
                {
                    var xi1 = (-eqB + Mathf.Sqrt(d)) / (2 * eqA);
                    var xi2 = (-eqB - Mathf.Sqrt(d)) / (2 * eqA);

                    var yi1 = s1 * xi1 + s2;
                    var yi2 = s1 * xi2 + s2;

                    if (IsPointOnLine(new(a.x, a.y), new(b.x, b.y), new(xi1, yi1)))
                    {
                        intersections.Add(new(xi1 + c.x, yi1 + c.y));
                    }
                    if (IsPointOnLine(new(a.x, a.y), new(b.x, b.y), new(xi2, yi2)))
                    {
                        intersections.Add(new(xi2 + c.x, yi2 + c.y));
                    }
                }
                else if (d == 0)
                {
                    var xi = -eqB / (2 * eqA);
                    var yi = s1 * xi + s2;

                    if (IsPointOnLine(new(a.x, a.y), new(b.x, b.y), new(xi, yi)))
                    {
                        intersections.Add(new(xi + c.x, yi + c.y));
                    }
                }
            }

            return intersections.ToArray();
        }

        public static bool IsPointOnLine(Vector2 a, Vector2 b, Vector2 p)
        {
            var xMin = Mathf.Min(a.x, b.x);
            var xMax = Mathf.Max(a.x, b.x);

            var yMin = Mathf.Min(b.x, b.y);
            var yMax = Mathf.Max(b.x, b.y);

            return (xMin <= p.x && p.x <= xMax) && (yMin <= p.y && p.y <= yMax);
        }

        public static bool LineIntersectsRectangle(Vector2 a, Vector2 b, Vector2 center, Vector2 size)
        {
            (Vector2 a, Vector2 b) bottomEdge = (new(center.x - size.x / 2, center.y - size.y / 2), new(center.x + size.x / 2, center.y - size.y / 2));
            if (LineIntersectsLine(a, b, bottomEdge.a, bottomEdge.b)) return true;

            (Vector2 a, Vector2 b) topEdge = (new(center.x - size.x / 2, center.y + size.y / 2), new(center.x + size.x / 2, center.y + size.y / 2));
            if (LineIntersectsLine(a, b, topEdge.a, topEdge.b)) return true;

            (Vector2 a, Vector2 b) leftEdge = (new(center.x - size.x / 2, center.y - size.y / 2), new(center.x - size.x / 2, center.y + size.y / 2));
            if (LineIntersectsLine(a, b, leftEdge.a, leftEdge.b)) return true;

            (Vector2 a, Vector2 b) rightEdge = (new(center.x + size.x / 2, center.y - size.y / 2), new(center.x + size.x / 2, center.y + size.y / 2));
            if (LineIntersectsLine(a, b, rightEdge.a, rightEdge.b)) return true;

            return false;
        }

        public static bool LineIntersectsLine(Vector2 a1, Vector2 b1, Vector2 a2, Vector2 b2)
        {
            return IsCounterClockwise(a1, a2, b2) != IsCounterClockwise(b1, a2, b2) && IsCounterClockwise(a1, b1, a2) != IsCounterClockwise(a1, b1, b2);
        }

        public static bool IsCounterClockwise(Vector2 a, Vector2 b, Vector2 c)
        {
            return (c.y - a.y) * (b.x - a.x) > (b.y - a.y) * (c.x - a.x);
        }
    }
}