using Mirror.Examples.Chat;
using UnityEngine;

public class ThirdPersonMovements : MonoBehaviour
{
    public CharacterController playerController;
    public Transform CameraTransform;
    
    public float defaultSpeed = 12f;
    public float sprintingSpeed = 18f;
    public float crouchingSpeed = 8f; // TODO : crouching movements
    public float gravity = -9.81f * 2;
    public float jumpHeight = 3f;
    public float turnSmoothTime = 0.1f;
 
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;
 
    private Vector3 _velocity;
    private bool _isGrounded;
    private float _speed;
    private bool _isCrouching; // TODO : crouching movements
    private float _turnSmoothVelocity;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        _speed = defaultSpeed;
        //Locking the cursor to the middle of the screen and making it invisible
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void PlayerMovements()
    {
        //checking if we hit the ground to reset our falling velocity, otherwise we will fall faster the next time
        _isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
 
        if (_isGrounded && _velocity.y < 0)
        {
            _velocity.y = -2f;
        }

        // handle sprinting : switch between default speed and sprinting speed
        _speed = Input.GetKey("left shift") ? sprintingSpeed : defaultSpeed;
        
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
 
        //right is the red Axis, foward is the blue axis
        Vector3 direction = new Vector3(x, 0f, z).normalized;

        if (direction != Vector3.zero)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + CameraTransform.eulerAngles.y;
            float smoothedAngle =
                Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, smoothedAngle, 0f);

            Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            
            playerController.Move(moveDirection.normalized * (_speed * Time.deltaTime));
        }
            
 
        //check if the player is on the ground so he can jump
        if (Input.GetButtonDown("Jump") && _isGrounded)
        {
            //the equation for jumping
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
 
        _velocity.y += gravity * Time.deltaTime;
 
        playerController.Move(_velocity * Time.deltaTime);
    }

    // Update is called once per frame
    private void Update()
    {
        PlayerMovements();
    }
}
