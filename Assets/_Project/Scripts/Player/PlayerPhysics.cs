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
            Debug.Log("dd");
            if (collidable.Collider is BoxCollider boxCollider) CheckBoxCollision(boxCollider);
        }

        private void CheckBoxCollision(BoxCollider collider)
        {
            var vertices = collider.GetWorldVertices();
            foreach (var vertex in vertices)
            {
                var vertexTopDownPosition = new Vector2(vertex.x, vertex.z);
                var playerTopDownPosition = new Vector2(transform.position.x, transform.position.z);

                if (Vector2.Distance(vertexTopDownPosition, playerTopDownPosition) > radius) continue;

                Debug.Log("Collision!");
            }
        }
    }
}