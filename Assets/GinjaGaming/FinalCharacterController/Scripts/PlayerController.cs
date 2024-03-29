using Cinemachine.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GinjaGaming.FinalCharacterController
{
    [DefaultExecutionOrder(-1)]
    public class PlayerController : MonoBehaviour
    {
        #region Class Variables
        [Header("Components")]
        [SerializeField] private CharacterController _characterController;
        [SerializeField] private Camera _playerCamera;
        public float RotationMismatch { get; private set; } = 0f;
        public bool IsRotatingToTarget { get; private set; } = false;

        [Header("Base Movement")]
        public float walkAcceleration = 0.15f;
        public float walkSpeed = 3f;
        public float runAcceleration = 0.25f;
        public float runSpeed = 6f;
        public float sprintAcceleration = 0.5f;
        public float sprintSpeed = 9f;
        public float drag = 0.1f;
        public float gravity = 25f;
        public float jumpSpeed = 1.0f;
        public float movingThreshold = 0.01f;
        public float edgeSlideSpeed = 1f;

        [Header("Animation")]
        public float playerModelRotationSpeed = 10f;
        public float rotateToTargetTime = 0.67f;

        [Header("Camera Settings")]
        public float lookSenseH = 0.1f;
        public float lookSenseV = 0.1f;
        public float lookLimitV = 89f;

        [Header("Environment Details")]
        [SerializeField] private LayerMask _groundLayers;

        private PlayerLocomotionInput _playerLocomotionInput;
        private PlayerState _playerState;

        private Vector2 _cameraRotation = Vector2.zero;
        private Vector2 _playerTargetRotation = Vector2.zero;

        private bool _jumpedLastFrame = false;
        private bool _isRotatingClockwise = false;
        private float _rotatingToTargetTimer = 0f;
        private float _verticalVelocity = 0f;
        private float _stepOffset = 0.3f;
        #endregion

        #region Startup
        private void Awake()
        {
            _playerLocomotionInput = GetComponent<PlayerLocomotionInput>();
            _playerState = GetComponent<PlayerState>();
        }
        #endregion

        #region Update Logic
        private void Update()
        {
            UpdateMovementState();
            HandleVerticalMovement();
            HandleLateralMovement();
        }

        private void UpdateMovementState()
        {
            bool canRun = CanRun();
            bool isMovementInput = _playerLocomotionInput.MovementInput != Vector2.zero;             //order
            bool isMovingLaterally = IsMovingLaterally();                                            //matters
            bool isSprinting = _playerLocomotionInput.SprintToggledOn && isMovingLaterally;          //order
            bool isWalking = (isMovingLaterally && !canRun) || _playerLocomotionInput.WalkToggledOn; //matters
            bool isGrounded = IsGrounded();

            PlayerMovementState lateralState = isWalking ? PlayerMovementState.Walking :
                                               isSprinting ? PlayerMovementState.Sprinting :
                                               isMovingLaterally || isMovementInput ? PlayerMovementState.Running : PlayerMovementState.Idling;

            _playerState.SetPlayerMovementState(lateralState);

            // Control Airborn State
            if ((!isGrounded || _jumpedLastFrame) && _characterController.velocity.y > 0f)
            {
                _playerState.SetPlayerMovementState(PlayerMovementState.Jumping);
                _jumpedLastFrame = false;
            }
            else if ((!isGrounded || _jumpedLastFrame) && _characterController.velocity.y <= 0f)
            {
                _playerState.SetPlayerMovementState(PlayerMovementState.Falling);
                _jumpedLastFrame = false;
            }
        }

        private void HandleVerticalMovement()
        {
            bool isGrounded = _playerState.InGroundedState();

            _verticalVelocity -= gravity * Time.deltaTime;

            if (isGrounded && _verticalVelocity < 0)
                _verticalVelocity = -2f;

            if (_playerLocomotionInput.JumpPressed && isGrounded)
            {
                _verticalVelocity += Mathf.Sqrt(jumpSpeed * 3 * gravity);
                _jumpedLastFrame = true;
            }
        }

        private void HandleLateralMovement()
        {
            // Create quick references for current state
            bool isSprinting = _playerState.CurrentPlayerMovementState == PlayerMovementState.Sprinting;
            bool isGrounded = _playerState.InGroundedState();
            bool isWalking = _playerState.CurrentPlayerMovementState == PlayerMovementState.Walking;

            // State dependent acceleration and speed
            float lateralAcceleration = isWalking ? walkAcceleration :
                                               isSprinting ? sprintAcceleration :
                                               !isGrounded ? runAcceleration : runAcceleration;

            float clampLateralMagnitude = isWalking ? walkSpeed :
                                                  isSprinting ? sprintSpeed :
                                                  !isGrounded ? sprintSpeed : runSpeed; // want to clamp at max speed in air (sprint speed)

            Vector3 cameraForwardXZ = new Vector3(_playerCamera.transform.forward.x, 0f, _playerCamera.transform.forward.z).normalized;
            Vector3 cameraRightXZ = new Vector3(_playerCamera.transform.right.x, 0f, _playerCamera.transform.right.z).normalized;
            Vector3 movementDirection = cameraRightXZ * _playerLocomotionInput.MovementInput.x + cameraForwardXZ * _playerLocomotionInput.MovementInput.y;

            Vector3 movementDelta = movementDirection * lateralAcceleration * Time.deltaTime;
            Vector3 newVelocity = _characterController.velocity;
            Vector3 verticalVelocity = new Vector3(0f, _verticalVelocity, 0f);

            newVelocity += movementDelta;

            if (_jumpedLastFrame || !isGrounded)
            {
                print(movementDelta);
                print($"Char {_characterController.velocity}");
                print($"Vel {newVelocity}");
            }


            // Add drag to player
            Vector3 currentDrag = newVelocity.normalized * drag * Time.deltaTime;
            newVelocity = (newVelocity.magnitude > drag * Time.deltaTime) ? newVelocity - currentDrag : Vector3.zero;
            newVelocity = Vector3.ClampMagnitude(new Vector3(newVelocity.x, 0f, newVelocity.z), clampLateralMagnitude);
            newVelocity += verticalVelocity;

            // Move character (Unity suggests only calling this once per tick)
            _characterController.Move(newVelocity * Time.deltaTime);
        }
        #endregion

        #region Late Update Logic
        private void LateUpdate()
        {
            UpdateCameraRotation();
        }

        private void UpdateCameraRotation()
        {
            _cameraRotation.x += lookSenseH * _playerLocomotionInput.LookInput.x;
            _cameraRotation.y = Mathf.Clamp(_cameraRotation.y - lookSenseV * _playerLocomotionInput.LookInput.y, -lookLimitV, lookLimitV);

            _playerTargetRotation.x += transform.eulerAngles.x + lookSenseH * _playerLocomotionInput.LookInput.x;

            float rotationTolerance = 90f;
            bool isIdling = _playerState.CurrentPlayerMovementState == PlayerMovementState.Idling;
            IsRotatingToTarget = _rotatingToTargetTimer > 0;

            // ROTATE if we're not idling
            if (!isIdling)
            {
                RotatePlayerToTarget();
            }
            // If rotation mismatch not within tolerance, or rotate to target is active, ROTATE
            else if (Mathf.Abs(RotationMismatch) > rotationTolerance || IsRotatingToTarget)
            {
                UpdateIdleRotation(rotationTolerance);
            }

            _playerCamera.transform.rotation = Quaternion.Euler(_cameraRotation.y, _cameraRotation.x, 0f);

            // Get angle between camera and player
            Vector3 camForwardProjectedXZ = new Vector3(_playerCamera.transform.forward.x, 0f, _playerCamera.transform.forward.z).normalized;
            Vector3 crossProduct = Vector3.Cross(transform.forward, camForwardProjectedXZ);
            float sign = Mathf.Sign(Vector3.Dot(crossProduct, transform.up));
            RotationMismatch = sign * Vector3.Angle(transform.forward, camForwardProjectedXZ);
        }

        private void UpdateIdleRotation(float rotationTolerance)
        {
            // Initiate new rotation direction
            if (Mathf.Abs(RotationMismatch) > rotationTolerance)
            {
                _rotatingToTargetTimer = rotateToTargetTime;
                _isRotatingClockwise = RotationMismatch > rotationTolerance;
            }
            _rotatingToTargetTimer -= Time.deltaTime;

            // Rotate player
            if (_isRotatingClockwise && RotationMismatch > 0f ||
                !_isRotatingClockwise && RotationMismatch < 0f)
            {
                RotatePlayerToTarget();
            }
        }

        private void RotatePlayerToTarget()
        {
            Quaternion targetRotationX = Quaternion.Euler(0f, _playerTargetRotation.x, 0f);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotationX, playerModelRotationSpeed * Time.deltaTime);
        }
        #endregion

        #region State Checks
        private bool IsMovingLaterally()
        {
            Vector3 lateralVelocity = new Vector3(_characterController.velocity.x, 0f, _characterController.velocity.y);

            return lateralVelocity.magnitude > movingThreshold;
        }

        private bool IsGrounded()
        {
            bool grounded = _playerState.InGroundedState() ? 
                            GroundedCheck() : 
                            IsGroundedWhileAirborne();

            return grounded;
        }

        private bool GroundedCheck()
        {
            // set sphere position, with offset
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - 0.1f,
                transform.position.z);
            bool grounded = Physics.CheckSphere(spherePosition, _characterController.radius, _groundLayers,
                QueryTriggerInteraction.Ignore);

            return grounded;
        }

        private bool IsGroundedWhileAirborne()
        {
            return _characterController.isGrounded;
        }

        private bool CanRun()
        {
            // This means player is moving diagonally at 45 degrees or forward, if so, we can run
            return _playerLocomotionInput.MovementInput.y >= Mathf.Abs(_playerLocomotionInput.MovementInput.x);
        }
        #endregion
    }
}
