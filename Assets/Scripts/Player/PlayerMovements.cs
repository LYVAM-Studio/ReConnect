using System;
using UnityEngine;

public class PlayerMovements : MonoBehaviour
{
    public CharacterController controller;
    
    public float defaultSpeed = 12f;
    public float sprintingSpeed = 18f;
    public float crouchingSpeed = 8f; // TODO : crouching movements
    public float gravity = -9.81f * 2;
    public float jumpHeight = 3f;
 
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;
 
    private Vector3 _velocity;
    private bool _isGrounded;
    private float _speed;
    private bool _isCrouching; // TODO : crouching movements

    // Start is called once, at the beginning
    private void Start()
    {
        _speed = defaultSpeed;
    }

    // Update is called once per frame
    private void Update()
    {
        //checking if we hit the ground to reset our falling velocity, otherwise we will fall faster the next time
        _isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
 
        if (_isGrounded && _velocity.y < 0)
        {
            _velocity.y = -2f;
        }

        // handle sprinting : switch between default speed and sprinting speed
        _speed = Input.GetKey("left shift") ? sprintingSpeed : defaultSpeed;
        
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
 
        //right is the red Axis, foward is the blue axis
        Vector3 move = transform.right * x + transform.forward * z;
 
        controller.Move(move * (_speed * Time.deltaTime));
 
        //check if the player is on the ground so he can jump
        if (Input.GetButtonDown("Jump") && _isGrounded)
        {
            //the equation for jumping
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
 
        _velocity.y += gravity * Time.deltaTime;
 
        controller.Move(_velocity * Time.deltaTime);
    }
}
