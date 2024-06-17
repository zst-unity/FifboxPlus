using UnityEngine;

namespace Fifbox.Physics
{
    [RequireComponent(typeof(Collider))]
    public class PlayerCollidable : MonoBehaviour
    {
        public Collider Collider { get; private set; }

        private void Awake()
        {
            Collider = GetComponent<Collider>();
            FifboxPhysics.collidables.Add(this);
        }

        private void OnDestroy()
        {
            FifboxPhysics.collidables.Remove(this);
        }
    }
}
