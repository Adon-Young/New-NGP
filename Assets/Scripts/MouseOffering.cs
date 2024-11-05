using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class MouseOffering : NetworkBehaviour
{

    [SerializeField] private SpriteRenderer mouseSpriteRenderer;
    //new variables for more accurate movement
    private float mouseRotation = 0.0f;
    public float mouseMovementSpeed = 1.0f;
 
    private Vector3 previousMousePosition;
    private bool isMouseFlipped = false;

    //sync to server time variables...
    private float syncTimer;
    private float updateInterval = 0.5f;

    private void Start()
    {
        previousMousePosition = transform.position;//last known position = the platforms current position.
        mouseSpriteRenderer = GetComponent<SpriteRenderer>();
    }



    // Update is called once per frame
    void FixedUpdate()
    {
        if (IsServer)
        {
            MouseMovementFunction();//calling the movement if is server.(to make sure movement is synced across all players)
        }

        syncTimer = Time.deltaTime;
        if (Vector3.Distance(previousMousePosition, transform.position) > 0.01f)
        {
            MouseUpdateClientRPC(transform.position);
            previousMousePosition = transform.position;

        }
        syncTimer = 0f;

    }

    void MouseMovementFunction()
    {
        float newXPosition = Mathf.PingPong(Time.time * 1, 5);
        //moved the basic movement from class into its own function
        transform.position = new Vector3(newXPosition, transform.position.y, transform.position.z);
        UpdateSpriteFlip(newXPosition);//flipping the mouse for everyone
    }

    private void UpdateSpriteFlip( float newXPosition)
    {//not on button press but on value of position...
        if ((newXPosition >= 5f) && !isMouseFlipped)
        {
            Debug.Log("FlippedTrue");
            isMouseFlipped = true;
        }
        else if ((newXPosition <= 0f) && isMouseFlipped)
        {
            isMouseFlipped = false;
            Debug.Log("FlippedFalse");
        }

        mouseSpriteRenderer.flipX = isMouseFlipped;
    }



    [ClientRpc]

    void MouseUpdateClientRPC(Vector3 updatedPosition)
    {
        if (!IsServer) // Don't update on the server, it already has the correct position
        {
            transform.position = updatedPosition;
        }
    }


}
