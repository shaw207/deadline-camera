using UnityEngine;

namespace DeadLine
{
    // A scene can opt into directional room handling. Standalone camera
    // triggers have no dependency on that scene or on its concrete receiver.
    public abstract class CameraRouteReceiver : MonoBehaviour
    {
        public abstract bool Owns(CameraRouteTrigger trigger);
        public abstract void Contact(Collider other);
    }

    public abstract class CameraRouteTrigger : MonoBehaviour
    {
        private CameraRouteReceiver route;

        public void BindRoute(CameraRouteReceiver route)
        {
            this.route = route;
        }

        public abstract void Apply(CameraController controller);

        protected void OnTriggerEnter(Collider other)
        {
            if (route && route.Owns(this))
            {
                route.Contact(other);
                return;
            }

            if (!other.CompareTag("Player"))
                return;
            Camera camera = Camera.main;
            if (camera && camera.TryGetComponent(out CameraController controller))
                Apply(controller);
        }

        protected void OnTriggerStay(Collider other)
        {
            if (route && route.Owns(this))
                route.Contact(other);
        }

        protected void OnTriggerExit(Collider other)
        {
            if (route && route.Owns(this))
                route.Contact(other);
        }
    }
}
