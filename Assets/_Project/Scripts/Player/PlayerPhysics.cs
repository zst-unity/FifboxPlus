using System.Collections.Generic;
using Fifbox.Physics;
using UnityEngine;
using ZSToolkit.ZSTGizmos;

namespace Fifbox.Player
{
    public class PlayerPhysics : MonoBehaviour
    {
        [Header("Collider")]
        public float radius = 0.25f;
        public float height = 1.8f;

        private Vector3 ColliderPosition => transform.position + Vector3.up * height / 2;

        private Bounds Bounds => new
        (
            transform.position + Vector3.up * height / 2,
            new
            (
                radius * 2 * transform.lossyScale.x,
                height * transform.lossyScale.y,
                radius * 2 * transform.lossyScale.z
            )
        );

        private void OnDrawGizmos()
        {
            ZSTGizmos.DrawWireCylinder(ColliderPosition, Quaternion.identity, transform.lossyScale, radius, height, Color.green);
            Gizmos.DrawWireCube(Bounds.center, Bounds.size);
        }

        private void FixedUpdate()
        {
            CheckCollisions();
        }

        private void CheckCollisions()
        {
            foreach (var collidable in FifboxPhysics.collidables)
            {
                if (!collidable.Collider.bounds.Intersects(Bounds)) continue;
                CheckCollision(collidable);
            }
        }

        private void CheckCollision(PlayerCollidable collidable)
        {
            if (collidable.Collider is BoxCollider boxCollider) CheckBoxCollision(boxCollider);
        }

        private void CheckBoxCollision(BoxCollider collider)
        {
            var edges = collider.GetWorldEdges();
            var intersectsTop = false;
            var intersectsForward = false;
            var intersectsRight = false;

            foreach (var (a, b) in edges)
            {
                var ellipsePosition = new Vector2(transform.position.x, transform.position.z);
                var ellipseRadius = new Vector2(radius * transform.lossyScale.x, radius * transform.lossyScale.z);
                var ellipseIntersections = FifboxPhysicsUtility.GetEllipseLineIntersections(new(a.x, a.z), new(b.x, b.z), ellipsePosition, ellipseRadius);
                if (ellipseIntersections.Length > 0) intersectsTop = true;

                var forwardBackRectanglePosition = (Vector2)ColliderPosition;
                var forwardBackRectangleSize = new Vector2(radius * 2 * transform.lossyScale.x, height * transform.lossyScale.y);
                var forwardBackIntersects = FifboxPhysicsUtility.LineIntersectsRectangle(new(a.x, a.y), new(b.x, b.y), forwardBackRectanglePosition, forwardBackRectangleSize);
                if (forwardBackIntersects) intersectsForward = true;

                var leftRightRectanglePosition = new Vector2(ColliderPosition.z, ColliderPosition.y);
                var leftRightRectangleSize = new Vector2(radius * 2 * transform.lossyScale.z, height * transform.lossyScale.y);
                var leftRightIntersects = FifboxPhysicsUtility.LineIntersectsRectangle(new(a.z, a.y), new(b.z, b.y), leftRightRectanglePosition, leftRightRectangleSize);
                if (leftRightIntersects) intersectsRight = true;
            }

            if (intersectsTop && intersectsForward && intersectsRight) Debug.Log("Box Collision!");
        }
    }
}