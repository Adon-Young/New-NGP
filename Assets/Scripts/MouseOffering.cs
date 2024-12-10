using UnityEngine;
using Unity.Netcode;

public class MouseOffering : NetworkBehaviour
{
    [SerializeField] private SpriteRenderer mouseSpriteRenderer;
    [SerializeField] private NewPlayerController playerController;  // This will be assigned dynamically
    public enum MouseType { Fire, Water, Magic, Plant }
    public MouseType mouseType;

    public float mouseMovementSpeed = 1.0f;
    public float movementRange = 2.5f;

    private Vector3 startingPosition;
    private bool isMouseFlipped = false;
    private bool isPlayerControllerFound = false;

    private NetworkVariable<Vector3> mousePosition = new NetworkVariable<Vector3>();
    private NetworkVariable<bool> isFlipped = new NetworkVariable<bool>();

    private void Start()
    {
        startingPosition = transform.position;
        mouseSpriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {


        if (IsClient)
        {
            transform.position = mousePosition.Value;
            mouseSpriteRenderer.flipX = isFlipped.Value;
        }

        if (IsServer)
        {
            MouseMovementFunction();
        }

    }

    void MouseMovementFunction()
    {
        float newXPosition = startingPosition.x + Mathf.PingPong(Time.time * mouseMovementSpeed, movementRange * 2) - movementRange;
        Vector3 newPosition = new Vector3(newXPosition, startingPosition.y, transform.position.z);

        mousePosition.Value = newPosition;
        transform.position = newPosition;

        UpdateSpriteFlip(newXPosition);
    }

    private void UpdateSpriteFlip(float newXPosition)
    {
        float epsilon = 0.1f;

        if (newXPosition >= startingPosition.x + movementRange - epsilon && !isMouseFlipped)
        {
            isMouseFlipped = true;
            isFlipped.Value = true;
        }
        else if (newXPosition <= startingPosition.x - movementRange + epsilon && isMouseFlipped)
        {
            isMouseFlipped = false;
            isFlipped.Value = false;
        }
    }



}
