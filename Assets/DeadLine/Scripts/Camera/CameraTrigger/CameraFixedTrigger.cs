using UnityEngine;
using UnityEngine.Serialization;

namespace DeadLine
{
    public class CameraFixedTrigger : CameraRouteTrigger
    {
        [Header("Fixed Camera Setup")]
        [FormerlySerializedAs("fixedPoint")]
        [SerializeField] private Transform cameraPoint;   // camera position
        [SerializeField] private Transform lookPoint;     // camera look-at target 

        [Header("Look Behavior")]
        [SerializeField] private bool followPlayer = true;

        public override void Apply(CameraController controller)
        {
            if (cameraPoint)
                controller.SetFixedMode(cameraPoint, lookPoint, followPlayer);
        }
    }
}