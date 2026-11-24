using UnityEngine;

namespace DeadLine
{
    public class Player : Character
    {
        private InputManager _input;

        protected override void Start()
        {
            base.Start();
            _stateMachine = new PlayerStateMachineBuilder(this).Build();
            _input = InputManager.Instance;
            _input.DebugPressed += RespawnAtCheckpoint;
        }

        private void OnDestroy()
        {
            if (_input)
                _input.DebugPressed -= RespawnAtCheckpoint;
        }

        private static void RespawnAtCheckpoint()
        {
            if (CheckpointsManager.Instance)
                CheckpointsManager.Instance.RespawnPlayer();
        }

        protected override void Update()
        {
            base.Update();
            _stateMachine.Update();
        }

        private void FixedUpdate()
        {
            _stateMachine.FixedUpdate();
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying || _stateMachine?.GetState() == null)
                return;

            Vector3 labelPosition = transform.position + Vector3.up;
            UnityEditor.Handles.Label(labelPosition, "Current State: " + _stateMachine.GetState().GetType().Name);
        }
#endif
    }
}
