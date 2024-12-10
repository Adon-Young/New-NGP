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
        // Only search for the player controller if we haven't found it yet
        if (!isPlayerControllerFound)
        {
            FindPlayerController();
        }

        if (IsClient)
        {
            transform.position = mousePosition.Value;
            mouseSpriteRenderer.flipX = isFlipped.Value;
        }

        if (IsServer)
        {
            MouseMovementFunction();
        }

        if (isPlayerControllerFound)
        {
            UpdateMouseColor();
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

    private void UpdateMouseColor()
    {
        if (playerController != null)
        {
            Color playerColor = playerController.GetPlayerColor();

            if (IsMatchingMouseAndPlayer(playerController))
            {
                mouseSpriteRenderer.color = new Color(playerColor.r, playerColor.g, playerColor.b, 1f);
            }
            else
            {
                mouseSpriteRenderer.color = new Color(playerColor.r, playerColor.g, playerColor.b, 0.6f);
            }
        }
    }

    private bool IsMatchingMouseAndPlayer(NewPlayerController playerController)
    {
        switch (mouseType)
        {
            case MouseType.Fire:
                return playerController.worldType == NewPlayerController.WorldType.Fire;
            case MouseType.Water:
                return playerController.worldType == NewPlayerController.WorldType.Water;
            case MouseType.Magic:
                return playerController.worldType == NewPlayerController.WorldType.Magic;
            case MouseType.Plant:
                return playerController.worldType == NewPlayerController.WorldType.Plant;
            default:
                return false;
        }
    }

    // This method will dynamically search for the player controller based on the MouseType match
    private void FindPlayerController()
    {
        // Find all players tagged as "Player"
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (var player in players)
        {
            if (player.TryGetComponent(out NewPlayerController foundPlayerController))
            {
                // Check if the MouseType matches the WorldType of the found player
                if (IsMatchingMouseAndPlayer(foundPlayerController))
                {
                    playerController = foundPlayerController;  // Assign the found controller
                    isPlayerControllerFound = true;  // Stop searching once we find the correct player
                    break;  // Exit the loop
                }
            }
        }
    }
}
