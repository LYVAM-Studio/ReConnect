using System;
using System.Collections;
using Reconnect.Menu;
using Reconnect.Utils;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Reconnect.Player
{
    public class PlayerMovementsNetwork : PlayerNetwork
    {
        [Tooltip("The height the player should jump")]
        public float jumpHeight = 0.7f;

        [Header("Speed settings")]
        [Tooltip("The walking speed of the player")]
        public float defaultSpeed = 1f;

        [Tooltip("The sprinting speed modifier to be applied to the defaultSpeed")]
        public float sprintingFactor = 1.2f;
        [Tooltip("The crouching speed modifier to be applied to the defaultSpeed")]
        public float crouchingFactor = 0.7f;
        [Tooltip("The time to smooth the rotation of the player (camera and keyboard)")]
        public float turnSmoothTime = 0.1f;
        [Tooltip("The time tp smooth the rotation of the player while mid air")]
        public float turnSmoothTimeMidAir = 0.5f;
        [Tooltip("In air movement smooth")]
        private const float AirControlAcceleration = 8f;
        
        // movement
        private Vector3 _currentMovement; // the movement to be applies to the player
        private Vector3 _currentVelocity;
        private Vector2 _currentMovementInput;
        private int _isWalkingHash;
        private bool _isCrouching;
        private int _isCrouchingHash;
        private bool _isDancing;
        private int _isDancingHash;
        private int _isFallingHash;
        private int _isGroundedHash;
        private bool _isJumping;
        private int _isJumpingHash;
        private bool _isJumpingPressed;

        // states memory
        private bool _isMovementPressed;
        private bool _isRunning;
        private int _isRunningHash;

        // KO
        [NonSerialized] public bool IsKo;
        private int _isKoHash;
        
        // internal values
        private float _turnSmoothVelocity;

        // imported components
        
        // the animator component on the 3D model of the Player inside the current GameObject
        private Animator _animator;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        public override void Awake()
        {
            base.Awake();

            PlayerControls.Player.Move.started += OnMove;
            PlayerControls.Player.Move.performed += OnMove;
            PlayerControls.Player.Move.canceled += OnMove;
            PlayerControls.Player.Sprint.started += OnSprint;
            PlayerControls.Player.Sprint.canceled += OnSprint;
            PlayerControls.Player.Crouch.started += OnCrouch;
            PlayerControls.Player.Jump.started += OnJump;
            PlayerControls.Player.Dance.started += OnDance;

            if (!TryGetComponent(out _animator))
                throw new ComponentNotFoundException("No Animator component has been found on the player.");

            _isWalkingHash = Animator.StringToHash("isWalking");
            _isRunningHash = Animator.StringToHash("isRunning");
            _isCrouchingHash = Animator.StringToHash("isCrouching");
            _isJumpingHash = Animator.StringToHash("isJumping");
            _isFallingHash = Animator.StringToHash("isFalling");
            _isGroundedHash = Animator.StringToHash("isGrounded");
            _isDancingHash = Animator.StringToHash("isDancing");
            _isKoHash = Animator.StringToHash("isKo");
        }

        // Update is called once per frame
        private void Update()
        {
            if (!isLocalPlayer) return;
            if (isLocked) HandleLockedPlayer();
            HandleInputs();
            HandleGravityAndJump();
            HandleAnimation();
            HandleRotation2();
            HandleMovements();
        }

        private void OnEnable()
        {
            PlayerControls.Player.Enable();
        }

        private void OnDisable()
        {
            PlayerControls.Player.Disable();
        }

        public void OnDestroy()
        {
            // It's a good practice to unsubscribe from actions when the object is destroyed
            PlayerControls.Player.Move.started -= OnMove;
            PlayerControls.Player.Move.performed -= OnMove;
            PlayerControls.Player.Move.canceled -= OnMove;
            PlayerControls.Player.Sprint.started -= OnSprint;
            PlayerControls.Player.Sprint.canceled -= OnSprint;
            PlayerControls.Player.Crouch.started -= OnCrouch;
            PlayerControls.Player.Jump.started -= OnJump;
            PlayerControls.Player.Dance.started -= OnDance;
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            if (isLocked)
                return;
            _currentMovementInput = context.ReadValue<Vector2>();
            _currentMovement.x = _currentMovementInput.x;
            _currentMovement.z = _currentMovementInput.y;
            _isMovementPressed = _currentMovement != Vector3.zero;
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (isLocked)
                return;

            _isJumpingPressed = context.ReadValue<float>() != 0f;
        }

        public void OnCrouch(InputAction.CallbackContext context)
        {
            if (isLocked)
                return;

            _isCrouching = !_isCrouching;
        }

        public void OnSprint(InputAction.CallbackContext context)
        {
            if (isLocked || !CharacterController.isGrounded)
                return;

            if (context.started)
            {
                _isRunning = true;
                _isCrouching = false;
            }
            else if (context.canceled)
            {
                _isRunning = false;
            }
        }

        public void OnDance(InputAction.CallbackContext context)
        {
            if (isLocked)
                return;

            _isDancing = true;
        }

        private void HandleInputs()
        {
            if (_isJumpingPressed && _isCrouching)
            {
                _isCrouching = false; // jumping cancels crouching
                _isJumpingPressed = false;
            }
            
            if (_isJumpingPressed && !CharacterController.isGrounded)
            {
                _isJumpingPressed = false; // cancel double jump
            }
        }

        private void JumpAnimation()
        {
            if (_isJumpingPressed && !_isJumping)
            {
                _animator.SetBool(_isJumpingHash, true);
                _isJumping = true;
                _isJumpingPressed = false;
            }
            else if (CharacterController.isGrounded) // is character grounded, no more falling nor jumping
            {
                _animator.SetBool(_isGroundedHash, true);
                _animator.SetBool(_isJumpingHash, false);
                _isJumping = false;
                _animator.SetBool(_isFallingHash, false);
            }
            else // if the character is not grounded, then it is maybe falling
            {
                _animator.SetBool(_isGroundedHash, false);

                // if not grounded, it's falling if it's on the descending part of the jump or if it fell from a height (with velocityY threshold of -2f)
                if ((_currentVelocity.y < 0 && _isJumping) || _currentVelocity.y < -2f) _animator.SetBool(_isFallingHash, true);
            }
        }

        private void HandleAnimation()
        {
            var isWalking = _animator.GetBool(_isWalkingHash);
            var isRunning = _animator.GetBool(_isRunningHash);
            var isCrouching = _animator.GetBool(_isCrouchingHash);
            var isDancing = _animator.GetBool(_isDancingHash);
            JumpAnimation();

            if (_isDancing && !isDancing && !_isMovementPressed)
                _animator.SetBool(_isDancingHash, true);

            if (isDancing && _isMovementPressed)
            {
                _animator.SetBool(_isDancingHash, false);
                _isDancing = false;
            }

            if (_isMovementPressed && !isWalking)
                _animator.SetBool(_isWalkingHash, true);

            if (_isRunning && !isRunning)
                _animator.SetBool(_isRunningHash, true);

            if (_isCrouching && !isCrouching)
                _animator.SetBool(_isCrouchingHash, true);

            if (!_isMovementPressed && isWalking)
                _animator.SetBool(_isWalkingHash, false);

            // stop running if key released
            if (!_isRunning && isRunning) _animator.SetBool(_isRunningHash, false);

            // stop running if no more moving (even though the key is still pressed)
            if (!_isMovementPressed && isRunning)
                _animator.SetBool(_isRunningHash, false);

            if (!_isCrouching && isCrouching)
                _animator.SetBool(_isCrouchingHash, false);
        }

        private void HandleRotation2()
        {
            if (isLocked)
                return;

            // Extract input directions
            var x = _currentMovementInput.x;
            var z = _currentMovementInput.y;

            // Combine the input into a direction vector
            var direction = new Vector3(x, 0f, z).normalized;

            // Only process movement when there's a significant input
            if (direction.magnitude >= 0.1f)
            {
                // Calculate target rotation based on camera orientation
                var targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg +
                                  FreeLookCamera.Transform.eulerAngles.y;

                // Smooth the player's rotation
                var smoothedAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle,
                    ref _turnSmoothVelocity, (CharacterController.isGrounded ? turnSmoothTime : turnSmoothTimeMidAir));
                transform.rotation = Quaternion.Euler(0f, smoothedAngle, 0f);

                // Calculate movement direction relative to the player's rotation
                _currentMovement = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            }
        }

        private void HandleMidAirMovements(float speed)
        {
            Vector3 inputDirection = new Vector3(_currentMovement.x, 0, _currentMovement.z).normalized;
            Vector3 desiredVelocity = inputDirection * speed;

            if (!CharacterController.isGrounded)
            {
                // Smoothly accelerate toward desiredVelocity in air
                _currentVelocity.x = Mathf.Lerp(_currentVelocity.x, desiredVelocity.x, AirControlAcceleration * Time.deltaTime);
                _currentVelocity.z = Mathf.Lerp(_currentVelocity.z, desiredVelocity.z, AirControlAcceleration * Time.deltaTime);
            }
            else
            {
                // Grounded : directly set horizontal movement
                _currentVelocity.x = desiredVelocity.x;
                _currentVelocity.z = desiredVelocity.z;
            }
        }

        private void HandleMovements()
        {
            var speed = defaultSpeed;
            if (_isRunning)
                speed *= sprintingFactor;
            else if (_isCrouching)
                speed *= crouchingFactor;

            HandleMidAirMovements(speed);
            
            CharacterController.Move(_currentVelocity * Time.deltaTime);
        }

        private void HandleGravityAndJump()
        {
            if (CharacterController.isGrounded && _isJumpingPressed) // jumping vertical velocity
                _currentVelocity.y = Mathf.Sqrt(jumpHeight * -2f * Physics.Gravity);
            else if (CharacterController.isGrounded) // on ground vertical velocity
                _currentVelocity.y = Physics.Gravity;
            else
                _currentVelocity.y += Physics.Gravity * Time.deltaTime;
        }

        private void HandleLockedPlayer()
        {
            // overwrite the movement to avoid it from moving
            _currentMovement.x = 0;
            _currentMovement.z = 0;
            _isMovementPressed = false;
            _currentMovementInput = Vector2.zero;
        }

        private IEnumerator KoDelay()
        {
            IsKo = true;
            isLocked = true;
            FreeLookCamera.InputAxisController.enabled = false;
            _animator.SetBool(_isKoHash, true);
            yield return MenuManager.Instance.KnockOutForSeconds(10);
            _animator.SetBool(_isKoHash, false);
        }

        public void OnEndKo()
        {
            FreeLookCamera.InputAxisController.enabled = true;
            isLocked = false;
            IsKo = false;
        }
        public void KnockOut()
        {
            if (!IsKo)
                StartCoroutine(KoDelay());
        }

        public void CancelKnockOut()
        {
            if (!IsKo) return;
            StopCoroutine(nameof(KoDelay));
            _animator.SetBool(_isKoHash, false);
        }
    }
}