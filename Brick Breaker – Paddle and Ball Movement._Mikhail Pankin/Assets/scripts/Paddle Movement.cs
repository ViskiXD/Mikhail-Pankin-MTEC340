using System;
using UnityEngine;

public class PaddleMovement : MonoBehaviour
{
    public KeyCode RightDirection;
    public KeyCode LeftDirection;

    public float LimitY = 8.5f;

    public float LimitX = 0f;



    public float moveSpeed = 10.0f;

    // Update is called once per frame
    void Update()
    {
        float movement = 0.0f;
        // Update if the given Keycode is pressed
        if (Input.GetKey(RightDirection))
        {
            movement = 1.0f;
        }
        if (Input.GetKey(LeftDirection))
        {
            movement = -1.0f;
        }
        
        Vector3 newPosition = transform.position + new Vector3(movement, 0.0f, 0.0f) * Time.deltaTime * moveSpeed;

        
        newPosition.x = Mathf.Clamp(newPosition.x, -LimitX, LimitX);    

        
        transform.position = newPosition;


    }
}
