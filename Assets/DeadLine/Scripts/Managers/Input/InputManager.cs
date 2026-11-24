using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DeadLine
{
    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance;

        private PlayerInputActions _inputActions;

        private bool _isMovementPressed, _isJumpPressed, _isCrouchPressed, _isSprintPressed, _isPausePressed;

        public Action CrouchPressed, CrouchCanceled, DebugPressed, InteractPressed, InteractCanceled, ThrowPressed, ThrowCanceled;

        public Action UiEscPressed;
        public Action UiEnterPressed;
        public Action UiSpacePressed;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else { Destroy(gameObject); return; }

            _inputActions = new PlayerInputActions();
        }

        private void OnEnable()
        {
            if (_inputActions == null)
                return;


            _inputActions.Player.Move.started += OnMoveStarted;
            _inputActions.Player.Move.canceled += OnMoveCanceled;

            _inputActions.Player.Crouch.started += OnCrouchStarted;
            _inputActions.Player.Crouch.canceled += OnCrouchCanceled;

            _inputActions.Player.Interact.started += OnInteractStarted;
            _inputActions.Player.Interact.canceled += OnInteractCanceled;

            _inputActions.Player.Debug.started += OnDebugStarted;

            _inputActions.Player.Jump.started += OnJumpStarted;
            _inputActions.Player.Jump.canceled += OnJumpCanceled;

            _inputActions.Player.Sprint.started += OnSprintStarted;
            _inputActions.Player.Sprint.canceled += OnSprintCanceled;

            _inputActions.Player.Throw.started += OnThrowStarted;
            _inputActions.Player.Throw.canceled += OnThrowCanceled;

            _inputActions.Ui.Esc.performed += OnUiEsc;
            _inputActions.Ui.Enter.performed += OnUiEnter;
            _inputActions.Ui.Space.performed += OnUiSpace;


            EnablePlayerInputActions();
        }

        private void OnDisable()
        {
            if (_inputActions == null)
                return;

            _inputActions.Player.Move.started -= OnMoveStarted;
            _inputActions.Player.Move.canceled -= OnMoveCanceled;

            _inputActions.Player.Crouch.started -= OnCrouchStarted;
            _inputActions.Player.Crouch.canceled -= OnCrouchCanceled;

            _inputActions.Player.Interact.started -= OnInteractStarted;
            _inputActions.Player.Interact.canceled -= OnInteractCanceled;

            _inputActions.Player.Debug.started -= OnDebugStarted;

            _inputActions.Player.Jump.started -= OnJumpStarted;
            _inputActions.Player.Jump.canceled -= OnJumpCanceled;

            _inputActions.Player.Sprint.started -= OnSprintStarted;
            _inputActions.Player.Sprint.canceled -= OnSprintCanceled;

            _inputActions.Player.Throw.started -= OnThrowStarted;
            _inputActions.Player.Throw.canceled -= OnThrowCanceled;

            _inputActions.Ui.Esc.performed -= OnUiEsc;
            _inputActions.Ui.Enter.performed -= OnUiEnter;
            _inputActions.Ui.Space.performed -= OnUiSpace;

            DisableAllInputActions();
        }

        private void OnDestroy()
        {
            _inputActions?.Dispose();
            if (Instance == this)
                Instance = null;
        }

        public void EnablePlayerInputActions()
        {
            DisableAllInputActions();
            _inputActions.Player.Enable();
        }

        public void EnableUiInputActions()
        {
            DisableAllInputActions();
            _inputActions.Ui.Enable();
        }

        private void DisableAllInputActions()
        {
            _inputActions.Player.Disable();
            _inputActions.Ui.Disable();
        }

        public Vector2 GetMovementInput() => _inputActions.Player.Move.ReadValue<Vector2>();
        public bool IsMoving() => GetMovementInput().magnitude > 0.01f;
        public Vector2 GetMousePosition() => _inputActions.Player.Mouse.ReadValue<Vector2>();

        public bool IsMovementPressed => _isMovementPressed;
        public bool IsJumpPressed { get => _isJumpPressed; set => _isJumpPressed = value; }
        public bool IsCrouchPressed => _isCrouchPressed;
        public bool IsSprintPressed => _isSprintPressed;
        public bool IsPausePressed => _isPausePressed;

        private void OnMoveStarted(InputAction.CallbackContext ctx) => _isMovementPressed = true;
        private void OnMoveCanceled(InputAction.CallbackContext ctx) => _isMovementPressed = false;

        private void OnCrouchStarted(InputAction.CallbackContext ctx) => CrouchPressed?.Invoke();
        private void OnCrouchCanceled(InputAction.CallbackContext ctx) => CrouchCanceled?.Invoke();

        private void OnInteractStarted(InputAction.CallbackContext ctx)
        {
            InteractPressed?.Invoke();    
        } 
        private void OnInteractCanceled(InputAction.CallbackContext ctx)
        {
            InteractCanceled?.Invoke();
        }

        private void OnThrowStarted(InputAction.CallbackContext ctx) => ThrowPressed?.Invoke();
        private void OnThrowCanceled(InputAction.CallbackContext ctx) => ThrowCanceled?.Invoke();

        private void OnDebugStarted(InputAction.CallbackContext ctx) => DebugPressed?.Invoke();

        private void OnJumpStarted(InputAction.CallbackContext ctx) => _isJumpPressed = true;
        private void OnJumpCanceled(InputAction.CallbackContext ctx) => _isJumpPressed = false;

        private void OnSprintStarted(InputAction.CallbackContext ctx) => _isSprintPressed = true;
        private void OnSprintCanceled(InputAction.CallbackContext ctx) => _isSprintPressed = false;


        private void OnUiEsc(InputAction.CallbackContext ctx) => UiEscPressed?.Invoke();
        private void OnUiEnter(InputAction.CallbackContext ctx) => UiEnterPressed?.Invoke();
        private void OnUiSpace(InputAction.CallbackContext ctx) => UiSpacePressed?.Invoke();
    }
}
