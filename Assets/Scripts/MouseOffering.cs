using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseOffering : MonoBehaviour
{
    [SerializeField] private SpriteRenderer mouseSpriteRenderer;

    public float mouseMovementSpeed = 1.0f;
    public float movementRange = 2.5f; // Half of the total movement range to either side

    private Vector3 startingPosition; // Center of ping-pong movement
    private bool isMouseFlipped = false;

    private void Start()
    {
        // Store the initial position of the mouse as the center of its ping-pong movement
        startingPosition = transform.position;
        mouseSpriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        MouseMovementFunction(); // Call movement in the Update method
    }

    void MouseMovementFunction()
    {
        // PingPong centered around the starting position, speed influenced by mouseMovementSpeed
        float newXPosition = startingPosition.x + Mathf.PingPong(Time.time * mouseMovementSpeed, movementRange * 2) - movementRange; // Adjust to center
        transform.position = new Vector3(newXPosition, startingPosition.y, transform.position.z);

        UpdateSpriteFlip(newXPosition); // Flip sprite if needed
    }

    private void UpdateSpriteFlip(float newXPosition)
    {
        float epsilon = 0.1f; // A small threshold for floating-point comparison

        // Flip based on the boundaries of movement
        if (newXPosition >= startingPosition.x + movementRange - epsilon && !isMouseFlipped) // Right boundary
        {
            isMouseFlipped = true;
            mouseSpriteRenderer.flipX = true; // Flip to face left
            Debug.Log("FlippedTrue");
        }
        else if (newXPosition <= startingPosition.x - movementRange + epsilon && isMouseFlipped) // Left boundary
        {
            isMouseFlipped = false;
            mouseSpriteRenderer.flipX = false; // Flip to face right
            Debug.Log("FlippedFalse");
        }
    }
}
