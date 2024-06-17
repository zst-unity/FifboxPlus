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
    }
}