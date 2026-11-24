using DeadLine;
using UnityEngine;

[DefaultExecutionOrder(-60)]
public sealed class CameraRouteBoundary : CameraRouteReceiver
{
    [SerializeField] private CameraController controller;
    [SerializeField] private Transform player;
    [SerializeField] private CameraBoundsTrigger boundsTrigger;
    [SerializeField] private CameraFixedTrigger fixedTrigger;
    [SerializeField] private CameraSplineTrigger splineTrigger;

    // Metres to either side of the centre plane. A foot resting on the seam
    // keeps its current room until the actor really crosses to the other side.
    private const float Hysteresis = 0.08f;
    private readonly Crossing[] crossings = new Crossing[3];
    private int currentZone = -1;
    private bool ready;

    private struct Crossing
    {
        public BoxCollider Volume;
        public Vector3 Centre;
        public Vector3 Forward;
        public Vector3 LastStablePosition;
        public bool Positive;
    }

    public void Configure(CameraController controller, Transform player,
        CameraBoundsTrigger bounds, CameraFixedTrigger fixedTrigger, CameraSplineTrigger spline)
    {
        this.controller = controller;
        this.player = player;
        boundsTrigger = bounds;
        this.fixedTrigger = fixedTrigger;
        splineTrigger = spline;

        if (Application.isPlaying && isActiveAndEnabled)
            Bind();
    }

    private void OnEnable()
    {
        if (Application.isPlaying)
            Bind();
    }

    private void OnDisable()
    {
        ready = false;
        currentZone = -1;
    }

    private void Bind()
    {
        ready = false;
        currentZone = -1;
        if (!controller || !player || !boundsTrigger || !fixedTrigger || !splineTrigger)
            return;

        CameraRouteTrigger[] triggers = { boundsTrigger, fixedTrigger, splineTrigger };
        for (int i = 0; i < triggers.Length; i++)
        {
            if (!triggers[i].TryGetComponent(out BoxCollider box))
                return;
            crossings[i].Volume = box;
            crossings[i].Centre = box.transform.TransformPoint(box.center);
        }

        Vector3 alongRoute = crossings[2].Centre - crossings[0].Centre;
        for (int i = 0; i < triggers.Length; i++)
        {
            BoxCollider box = crossings[i].Volume;
            Vector3 x = box.transform.TransformVector(Vector3.right * box.size.x);
            Vector3 y = box.transform.TransformVector(Vector3.up * box.size.y);
            Vector3 z = box.transform.TransformVector(Vector3.forward * box.size.z);
            Vector3 thinAxis = x.sqrMagnitude < y.sqrMagnitude ? x : y;
            if (z.sqrMagnitude < thinAxis.sqrMagnitude)
                thinAxis = z;
            if (thinAxis.sqrMagnitude < 0.000001f)
                return;
            Vector3 normal = thinAxis.normalized;
            crossings[i].Forward = Vector3.Dot(normal, alongRoute) < 0f ? -normal : normal;
            crossings[i].LastStablePosition = player.position;
            crossings[i].Positive = SignedDistance(crossings[i], player.position) > 0f;
        }

        for (int i = 0; i < triggers.Length; i++)
            triggers[i].BindRoute(this);
        ready = true;
    }

    private static float SignedDistance(Crossing crossing, Vector3 position)
    {
        return Vector3.Dot(position - crossing.Centre, crossing.Forward);
    }

    public override bool Owns(CameraRouteTrigger trigger)
    {
        return ready && isActiveAndEnabled
            && (trigger == boundsTrigger || trigger == fixedTrigger || trigger == splineTrigger);
    }

    public override void Contact(Collider other)
    {
        if (!ready || !player || !other)
            return;
        Transform body = other.attachedRigidbody ? other.attachedRigidbody.transform : other.transform;
        if (body == player || body.IsChildOf(player) || other.transform.IsChildOf(player))
            Sample();
    }

    private void LateUpdate()
    {
        // A thin collider may be skipped by discrete physics between two steps.
        // Check the travelled segment against that same finite volume as a fallback.
        if (ready && player && controller)
            Sample();
    }

    private void Sample()
    {
        Vector3 position = player.position;
        if (currentZone < 0)
        {
            int startingZone = 0;
            for (int i = 0; i < crossings.Length; i++)
            {
                if (crossings[i].Positive)
                    startingZone = i + 1;
            }
            ApplyZone(startingZone);
        }

        int destination = -1;
        float nearestRemaining = float.PositiveInfinity;
        for (int i = 0; i < crossings.Length; i++)
        {
            ref Crossing crossing = ref crossings[i];
            float now = SignedDistance(crossing, position);
            if (Mathf.Abs(now) < Hysteresis)
                continue;

            bool positive = now > 0f;
            if (positive != crossing.Positive)
            {
                float before = SignedDistance(crossing, crossing.LastStablePosition);
                float t = Mathf.Clamp01(-before / (now - before));
                Vector3 intersection = Vector3.Lerp(crossing.LastStablePosition, position, t);
                BoxCollider volume = crossing.Volume;
                if (volume && volume.enabled && volume.gameObject.activeInHierarchy
                    && (volume.ClosestPoint(intersection) - intersection).sqrMagnitude < 0.0001f)
                {
                    float remaining = (position - intersection).sqrMagnitude;
                    if (remaining < nearestRemaining)
                    {
                        nearestRemaining = remaining;
                        destination = positive ? i + 1 : i;
                    }
                }
            }

            crossing.Positive = positive;
            crossing.LastStablePosition = position;
        }

        if (destination >= 0)
            ApplyZone(destination);
    }

    private void ApplyZone(int zone)
    {
        if (currentZone == zone || !controller)
            return;
        currentZone = zone;
        switch (zone)
        {
            case 0:
                controller.SetBounds(null, null);
                controller.SetFollowMode();
                break;
            case 1:
                boundsTrigger.Apply(controller);
                break;
            case 2:
                fixedTrigger.Apply(controller);
                break;
            case 3:
                splineTrigger.Apply(controller);
                break;
        }
    }
}
