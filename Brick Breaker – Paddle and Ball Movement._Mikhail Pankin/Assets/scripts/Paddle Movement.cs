using System;
using System.Runtime.CompilerServices;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PaddleMovement : MonoBehaviour
{
    [SerializeField] private float _speed = 5.0f;

    private float _direction = 0.0f;
    [SerializeField] private KeyCode _RightDirection;
    [SerializeField] private KeyCode _LeftDirection;

    private Rigidbody2D _rb;

    void Start()
    {
        // Get a reference to the rigidbody2D component
        _rb = GetComponent<Rigidbody2D>();
        // Disable gravity
        _rb.linearDamping = 0.0f;
        _rb.angularDamping = 0.0f;
        _rb.gravityScale = 0.0f;
    }

    void FixedUpdate()
    {
        // apply the direction and speed to the rigidbody2D component
        _rb.linearVelocity = new Vector2(_direction * _speed, _rb.linearVelocity.y);
    }

    void Update()
    {
        // Define the direction of the paddle
        _direction = 0.0f;

        if (Input.GetKey(_RightDirection)) _direction += 1.0f;
        if (Input.GetKey(_LeftDirection)) _direction -= 1.0f;
    
    }
}
