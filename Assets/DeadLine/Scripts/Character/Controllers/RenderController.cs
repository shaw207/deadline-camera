using System;
using System.Collections;
using UnityEngine;


namespace DeadLine
{
    [DefaultExecutionOrder(50)]
    public class RenderController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Animator _animator;
        [SerializeField] private SkinnedMeshRenderer _meshRenderer;
        
        // A presentation-only child. The root, capsule and gameplay probes remain on physics time.
        [SerializeField] private Transform _visualsRoot;
        private Vector3 _previousPhysicsPosition;
        private Vector3 _currentPhysicsPosition;
        private Vector3 _visualsLocalOffset;

        private Character _character;
        private int _currentAnimation;
        private Coroutine _changeAnimationCoroutine;

        private const float DEFAULT_CROSSFADE_DURATION = 0.2f;

        
        private void Awake()
        {
            if (_visualsRoot) _visualsLocalOffset = _visualsRoot.localPosition;

            _character = GetComponent<Character>();
            
            if (_animator == null)
                _animator = GetComponentInChildren<Animator>();
            
            if (_meshRenderer == null)
                _meshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        }

        private void OnEnable()
        {
            _previousPhysicsPosition = _currentPhysicsPosition = transform.position;
        }

        // Runs after the original physical movement. Character facing is already updated every render frame.
        private void FixedUpdate()
        {
            if (!_visualsRoot) return;
            _previousPhysicsPosition = _currentPhysicsPosition;
            _currentPhysicsPosition = transform.position;
        }

        private void LateUpdate()
        {
            if (!_visualsRoot) return;
            float alpha = Mathf.Clamp01((float)((Time.timeAsDouble - Time.fixedTimeAsDouble) / Time.fixedDeltaTime));
            Vector3 position = Vector3.Lerp(_previousPhysicsPosition, _currentPhysicsPosition, alpha);
            _visualsRoot.position = position + transform.TransformVector(_visualsLocalOffset);
        }

        private void OnDisable()
        {
            if (_visualsRoot)
                _visualsRoot.localPosition = _visualsLocalOffset;
        }

        private void Start()
        {
            if(_animator)
                _animator.applyRootMotion = false;
        }

        public void PlayAnimation(int animation)
        {
            if(!_animator) return;
            if(_changeAnimationCoroutine != null) StopCoroutine(_changeAnimationCoroutine);

            _currentAnimation = animation; 
            _animator.Play(animation);
        }

        public void ChangeAnimation(int animation, float crossfade = DEFAULT_CROSSFADE_DURATION, float fixedTimeOffset = 0f, int layer = -1)
        {
            if(!_animator || animation == _currentAnimation) return;
            if(_changeAnimationCoroutine != null) StopCoroutine(_changeAnimationCoroutine);

            _currentAnimation = animation;
            _animator.CrossFadeInFixedTime(animation, crossfade, layer, fixedTimeOffset);
        }

        public void ChangeAnimationWhenEnds(int animation, float crossfade = DEFAULT_CROSSFADE_DURATION, float fixedTimeOffset = 0f, int layer = -1)
        {
            if(!_animator || animation == _currentAnimation) return;
            
            AnimatorStateInfo animStateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            float remainingTime = animStateInfo.length * (1f - (animStateInfo.normalizedTime % 1f));

            ChangeAnimationWithDelay(remainingTime, animation, crossfade, fixedTimeOffset, layer);
        }

        public void ChangeAnimationWithDelay(float time, int animation, float crossfade = DEFAULT_CROSSFADE_DURATION, float fixedTimeOffset = 0f, int layer = -1)
        {
            if(!_animator || animation == _currentAnimation) return;
            if(_changeAnimationCoroutine != null) StopCoroutine(_changeAnimationCoroutine);

            _changeAnimationCoroutine = StartCoroutine(WaitAndChangeAnimation(time, animation, crossfade, fixedTimeOffset, layer));
        }

        private IEnumerator WaitAndChangeAnimation(float time, int animation, float crossfade = DEFAULT_CROSSFADE_DURATION, float fixedTimeOffset = 0f, int layer = -1)
        {
            float remainingTime = time - crossfade;
            if (remainingTime > 0)
                yield return new WaitForSeconds(remainingTime);
            
            _currentAnimation = animation;
            _animator.CrossFadeInFixedTime(animation, crossfade, layer, fixedTimeOffset);

            _changeAnimationCoroutine = null;
        }

    }

    [System.Serializable]
    public static class Animations{
        public static readonly int Idle = Animator.StringToHash("Breathing Idle");
        public static readonly int CrouchedIdle = Animator.StringToHash("Crouched");
        public static readonly int WalkingCrouched = Animator.StringToHash("Crouch Walk");
        public static readonly int HardLanding = Animator.StringToHash("Hard Landing");
        public static readonly int SoftLanding = Animator.StringToHash("Soft Landing");
        public static readonly int Pushing = Animator.StringToHash("Pushing");
        public static readonly int Pulling = Animator.StringToHash("Pulling");
        public static readonly int HangingLedge = Animator.StringToHash("Hanging Ledge");
        public static readonly int ClimbingLedge = Animator.StringToHash("Climbing Ledge");
        public static readonly int ClimbingLadder = Animator.StringToHash("Climbing Ladder");
        public static readonly int Throwing = Animator.StringToHash("Throwing");
        public static readonly int ThrowIdle = Animator.StringToHash("Throw Idle");
        public static readonly int Walking = Animator.StringToHash("Walking");
        public static readonly int Running = Animator.StringToHash("Running");
        public static readonly int JumpStart = Animator.StringToHash("JumpStart");
        public static readonly int Jumping = Animator.StringToHash("Jump");
        public static readonly int Airborn = Animator.StringToHash("Airborn");
        public static readonly int JumpEnd = Animator.StringToHash("JumpEnd");
        
    }
}