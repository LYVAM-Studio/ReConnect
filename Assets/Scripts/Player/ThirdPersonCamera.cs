using System;
using Mirror.Examples.Chat;
using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public float mouseSensitivity = 100f;
    public float rotationSpeed = 7f;
    private CharacterController _playerController;
    private Transform _orientation;
    private Transform _playerTransform;
    public GameObject player;
    
    
    float _xRotation = 0f;
    float _yRotation = 0f;
 
    void Start()
    {
        //Locking the cursor to the middle of the screen and making it invisible
        Cursor.lockState = CursorLockMode.Locked;
        _playerTransform = player.transform;
        _playerController = (CharacterController) player.GetComponent(typeof(CharacterController));
        _orientation = null;
        foreach (Transform child in _playerTransform)
        {
            if (child.name == "Orientation")
                _orientation = child.transform;
        }

        if (_orientation is null)
            throw new ArgumentException("Orientation gameobject not found");
    }
 
    void Update()
    {
        Vector3 viewDirection = _playerTransform.position -
                                new Vector3(transform.position.x, _playerTransform.position.y, transform.position.z);
        _orientation.forward = viewDirection.normalized;
        
        float verticalInput = Input.GetAxis("Horizontal");
        float horizontalInput = Input.GetAxis("Vertical");

        Vector3 inputDir = _orientation.forward * verticalInput + _orientation.right * horizontalInput;

        if (inputDir != Vector3.zero)
            _playerTransform.forward = Vector3.Slerp(_playerTransform.forward, inputDir.normalized,
                Time.deltaTime * rotationSpeed);

    }
}