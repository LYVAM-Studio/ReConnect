using System;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Reconnect.Player
{
    public class PlayerMovements : MonoBehaviour
    {
        // parameters that can be edited in Editor
        public float gravity = -9.81f; // gravity strength (default: -9.81, earth gravity)
        public float jumpHeight = 2.0f; // the height the player should jump
        public float defaultSpeed = 12.0f; // the walking speed of the player
        public float sprintingFactor = 1.5f; // the sprinting speed modifier to be applied to the defaultSpeed
        public float crouchingFactor = 0.7f; // the crouching speed modifier to be applied to the defaultSpeed
        public float turnSmoothTime = 0.1f; // the time to smooth the rotation of the player (camera and keyboard)
        
        // internal values
        private float _turnSmoothVelocity;
        private float _velocityY; // the velocity on the Y axis (jumping and falling)
        
        // imported components
        private PlayerInput _playerInput;
        private CharacterController _characterController;
        private Animator _animator; // the animator component on the 3D model of the Player inside the current GameObject
        private Transform _cameraTransform; // the MainCamera 3rd person inside the current GameObject
        
        // movement
        private Vector2 _currentMovementInput;
        private Vector3 _currentMovement; // the movement to be applies to the player
        
        // states memory
        private bool _isMovementPressed;
        private bool _isCrouching;
        private bool _isRunning;
        private bool _isJumpingPressed;
        private bool _isJumping;
        private bool _isFalling;
        private bool _isGrounded;
        
        // animations states hashes
        private int _isWalkingHash;
        private int _isRunningHash;
        private int _isCrouchingHash;
        private int _isJumpingHash;
        private int _isFallingHash;
        private int _isGroundedHash;
        
        
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Awake()
        {
            _playerInput = GetComponent<PlayerInput>();
            _playerInput.actions["Move"].started += OnMove;
            _playerInput.actions["Move"].performed += OnMove;
            _playerInput.actions["Move"].canceled += OnMove;
            _playerInput.actions["Sprint"].started += OnSprint;
            _playerInput.actions["Sprint"].canceled += OnSprint;
            _playerInput.actions["Crouch"].started += OnCrouch;
            _playerInput.actions["Jump"].started += OnJump;
            
            _characterController = GetComponent<CharacterController>();
            _animator = gameObject.FindComponentsInChildrenWithTag<Animator>("PlayerModel")[0] ??
                        throw new ArgumentException(
                            "There is no Animator component in the children of the current GameObject");
            
            _cameraTransform = gameObject.FindComponentsInChildrenWithTag<Transform>("MainCamera")[0] ??
                               throw new ArgumentException(
                                   "There is no MainCamera tagged gameObject in the children of the current GameObject");
            _isWalkingHash = Animator.StringToHash("isWalking");
            _isRunningHash = Animator.StringToHash("isRunning");
            _isCrouchingHash = Animator.StringToHash("isCrouching");
            _isJumpingHash = Animator.StringToHash("isJumping");
            _isFallingHash = Animator.StringToHash("isFalling");
            _isGroundedHash = Animator.StringToHash("isGrounded");
            //Locking the cursor to the middle of the screen and making it invisible
            Cursor.lockState = CursorLockMode.Locked;
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            _currentMovementInput = context.ReadValue<Vector2>();
            Debug.Log($"{_currentMovementInput.x}, {_currentMovementInput.y}");
            _currentMovement.x = _currentMovementInput.x;
            _currentMovement.z = _currentMovementInput.y;
            _isMovementPressed = _currentMovement != Vector3.zero;
        }
        
        public void OnJump(InputAction.CallbackContext context)
        {
            _isJumpingPressed = context.ReadValue<float>() != 0f;
        }

        public void OnCrouch(InputAction.CallbackContext context)
        {
            _isCrouching = !_isCrouching;
        }

        public void OnSprint (InputAction.CallbackContext context)
        {
            if (context.started)
                _isRunning = true;
            else if (context.canceled)
                _isRunning = false;
        }

        private void HandleInputs()
        {
            if (_isJumpingPressed && _isCrouching)
            {
                _isCrouching = false; // jumping cancels crouching
                _isJumpingPressed = false;
            }
        }

        private void JumpAnimation()
        {
            bool isJumping = _animator.GetBool(_isJumpingHash);
            bool isFalling = _animator.GetBool(_isFallingHash);
            bool isGrounded = _animator.GetBool(_isGroundedHash);

            if (_isJumpingPressed)
            {
                _animator.SetBool(_isJumpingHash, true);
                _isJumpingPressed = false;
            }
            else if (_characterController.isGrounded) // is character grounded, no more falling nor jumping
            {
                if (!isGrounded)
                {
                    _animator.SetBool(_isGroundedHash, true);
                    _isGrounded = true;
                }
                    
                if (isJumping)
                {
                    _animator.SetBool(_isJumpingHash, false);
                    _isJumping = false;
                }
                if (isFalling)
                {
                    _animator.SetBool(_isFallingHash, false);
                    _isFalling = false;
                }
                
            }
            else // if the character is not grounded, then it is maybe falling
            {
                if (isGrounded)
                {
                    _isGrounded = false;
                    _animator.SetBool(_isGroundedHash, false);
                }
                
                // if not grounded, it's falling if it's on the decending part of the jump or if it fell from a height (with velocityY threshold of -2f)
                if ((_velocityY < 0 && isJumping) || _velocityY < -2f)
                {
                    if (!isFalling)
                    {
                        _animator.SetBool(_isFallingHash, true);
                        _isFalling = true;
                    }
                }
            }
        }
        
        private void HandleAnimation()
        {
            bool isWalking = _animator.GetBool(_isWalkingHash);
            bool isRunning = _animator.GetBool(_isRunningHash);
            bool isCrouching = _animator.GetBool(_isCrouchingHash);
            
            JumpAnimation();
            
            if (_isMovementPressed && !isWalking)
                _animator.SetBool(_isWalkingHash, true);

            if (_isRunning && !isRunning)
                _animator.SetBool(_isRunningHash, true);

            if (_isCrouching && !isCrouching)
                _animator.SetBool(_isCrouchingHash, true);
            
            if (!_isMovementPressed && isWalking)
                _animator.SetBool(_isWalkingHash, false);

            if (!_isRunning && isRunning)
                _animator.SetBool(_isRunningHash, false);

            if (!_isCrouching && isCrouching)
                _animator.SetBool(_isCrouchingHash, false);
        }
        
        private void HandleRotation2()
        {
            // Extract input directions
            float x = _currentMovementInput.x;
            float z = _currentMovementInput.y;

            // Combine the input into a direction vector
            Vector3 direction = new Vector3(x, 0f, z).normalized;

            // Only process movement when there's a significant input
            if (direction.magnitude >= 0.1f)
            {
                // Calculate target rotation based on camera orientation
                float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg +
                                    _cameraTransform.eulerAngles.y;

                // Smooth the player's rotation
                float smoothedAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle,
                    ref _turnSmoothVelocity, turnSmoothTime);
                transform.rotation = Quaternion.Euler(0f, smoothedAngle, 0f);

                // Calculate movement direction relative to the player's rotation
                _currentMovement = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            }
        }

        private void HandleMovements()
        {
            float speed = defaultSpeed;
            if (_isRunning)
            {
                speed *= sprintingFactor;
            }
            else if (_isCrouching)
            {
                speed *= crouchingFactor;
            }
            
            Vector3 move = new Vector3(_currentMovement.x * speed, _velocityY, _currentMovement.z * speed);
            _characterController.Move(move * (Time.deltaTime ));
        }

        private void HandleGravityAndJump()
        {
            if (_characterController.isGrounded && _isJumpingPressed)
            {
                _velocityY = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
            _velocityY += gravity * Time.deltaTime;
        }
        
        // Update is called once per frame
        void Update()
        {
            HandleInputs();
            HandleGravityAndJump();
            HandleAnimation();
            HandleRotation2();
            HandleMovements();
            
        }

        private void OnEnable()
        {
            _playerInput.actions.Enable();
        }

        private void OnDisable()
        {
            _playerInput.actions.Disable();
        }

        void OnDestroy()
        {
            // It's a good practice to unsubscribe from actions when the object is destroyed
            _playerInput.actions["Move"].started -= OnMove;
            _playerInput.actions["Move"].performed -= OnMove;
            _playerInput.actions["Move"].canceled -= OnMove;
            _playerInput.actions["Sprint"].started -= OnSprint;
            _playerInput.actions["Sprint"].canceled -= OnSprint;
            _playerInput.actions["Crouch"].started -= OnCrouch;
            _playerInput.actions["Jump"].started -= OnJump;
        }
    }
}

