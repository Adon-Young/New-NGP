using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Unity.Netcode;
using static UnityEngine.GraphicsBuffer;

public class CameraFollower : NetworkBehaviour
{
    public Transform playerTarget; // The target the camera will follow
    private float smoothSpeed = 0.05f; // Smoothness of the camera movement
    public Vector3 offset; // Offset from the target position
    private void LateUpdate()
    {
        if (playerTarget)
        {
            Vector3 desiredPosition = playerTarget.position + offset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.position = smoothedPosition;

        }
    }

}
