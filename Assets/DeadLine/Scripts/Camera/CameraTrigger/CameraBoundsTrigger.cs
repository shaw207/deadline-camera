using UnityEngine;

namespace DeadLine
{
    public class CameraBoundsTrigger : CameraRouteTrigger
    {
        [SerializeField] private Transform minBound;
        [SerializeField] private Transform maxBound;
        [SerializeField] private bool forceFollowMode = true;

        public override void Apply(CameraController controller)
        {
            controller.SetBounds(minBound, maxBound);
            if (forceFollowMode)
                controller.SetFollowMode();
        }
    }
}