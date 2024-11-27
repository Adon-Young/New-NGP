using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;

public class NewPlayerController : NetworkBehaviour
{
    /*these bools will determing whihc local world game objects are activated for each player. this happens locally because we dont need
     * or want to send data over the network that doesnt need to be sent. for exampleif i had 1 sprite and i coded it to change colour
     * to match each players cat and sent it over the network that would be unnecessary as only the players locally will need to see those changes, therefore they dont need to communicate
     with the other players online. so better keeping it all local except the player movement and actions along with platforms and objects that are shared between worlds...*/
     public  bool isWaterWorld = false;
     public  bool isFireWorld = false;
     public  bool isPlantWorld = false;
     public  bool isMagicWorld = false;
     private static bool isFrozen = true;//instead of having it set to true for all over network testing it locally
     private bool gravityToggle = false;
    //------------------------------------------
    public bool isInWater = false;
    public float waterGravityScale = 2f; // Gravity scale while in water
    public float normalGravityScale = 1f; // Normal gravity scale
    public float waterMoveSpeed = 2f;
 
    //using properties allws me to keep the values private but still usable in my level controller script...

    //----------------------------------------------------

    [SerializeField] private GameObject spawnedObjectTransform;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private SpriteRenderer mouseSpriteRenderer;
    [SerializeField] private Text playerName;
    private GameObject mouseGameObject;

    public NetworkVariable<MyTransferrableData> onlinePlayerData =
    new NetworkVariable<MyTransferrableData>(new MyTransferrableData
    { }, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public NetworkVariable<bool> isSpriteFlipped = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private CameraFollower cameraFollow;
    public struct MyTransferrableData : INetworkSerializable
    {
        public FixedString128Bytes playerTag;
        public float rValue, gValue, bValue, aValue;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref playerTag);
            serializer.SerializeValue(ref rValue);
            serializer.SerializeValue(ref gValue);
            serializer.SerializeValue(ref bValue);
            serializer.SerializeValue(ref aValue);
        }

        public Color updateColour()
        {
            return new Color(rValue, gValue, bValue, aValue);
        }
    }

    public Rigidbody2D playersRB;
    public int moveSpeed;
    private bool isGrounded;
    public int jumpForce = 8;
    private Animator playerAnimatorController;
    public GameObject playerTagCanvas;

    public static void UnfreezePlayer()
    {
        isFrozen = false;
        Debug.Log("Player is unfrozen!");
    }



    public void Awake()
    {

        isFrozen = true;
        playersRB = GetComponent<Rigidbody2D>();
        playerAnimatorController = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        mouseGameObject = this.GameObject().transform.GetChild(1).gameObject;//getting the mouse game object child
        mouseSpriteRenderer = mouseGameObject.GetComponent<SpriteRenderer>(); //getting the sprite renderer of that mouse
        playersRB.freezeRotation = true;
        
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            cameraFollow = mainCamera.GetComponent<CameraFollower>();
        }

        isFrozen = true;

    }

    private void Update()
    {
        if (!IsOwner) return;

        playerTagCanvas.transform.rotation = Quaternion.identity;
        PlayerMovement();
        PlayerInput();
        UpdateSpriteFlip();

    }

    private void UpdateSpriteFlip()
    {
        if (Input.GetKey(KeyCode.A) && !isSpriteFlipped.Value && isFrozen == false)
        {
            isSpriteFlipped.Value = true;
        }
        else if (Input.GetKey(KeyCode.D) && isSpriteFlipped.Value && isFrozen == false)
        {
            isSpriteFlipped.Value = false;
        }

        spriteRenderer.flipX = isSpriteFlipped.Value;
        mouseSpriteRenderer.flipX = isSpriteFlipped.Value;


    }
    public void EnterWater()
    {
        isInWater = true;
        playersRB.gravityScale = waterGravityScale; // Disable gravity in water
    }

    public void ExitWater()
    {
        isInWater = false;
        playersRB.gravityScale = normalGravityScale; // Restore normal gravity
    }

    private void PlayerInput()
    {
        if (Input.GetMouseButtonDown(0) && isFrozen == false)
        {
            playerAnimatorController.SetBool("isInteracting", true);
            StartCoroutine(EndOfAnimation());
        }
        //---------------------------------------------------------------------------------------------------
        //input for [Fire] cat...
        //shooting fire ball...
        if (Input.GetMouseButtonDown(0) && !isFrozen && isFireWorld)
        {
            playerAnimatorController.SetBool("isInteracting", true);

            // Using isSpriteFlipped from PlayerController
            createBulletShotFromClientServerRpc(transform.position, transform.rotation, isSpriteFlipped.Value);
            StartCoroutine(EndOfAnimation());
        }

        //input for [Water] cat...
        if (Input.GetMouseButtonDown(0) && !isFrozen && isWaterWorld)
        {
            playerAnimatorController.SetBool("isInteracting", true);

            StartCoroutine(EndOfAnimation());
        }

        //input for [Plant] cat...
        if (Input.GetMouseButtonDown(0) && !isFrozen && isPlantWorld)
        {
            playerAnimatorController.SetBool("isInteracting", true);

            StartCoroutine(EndOfAnimation());
        }

        //input for [Magic] cat...
        //swapping gravity...
        if (Input.GetMouseButtonDown(0) && !isFrozen && isMagicWorld)
        {
            playerAnimatorController.SetBool("isInteracting", true);

            gravityToggle = !gravityToggle;

            if (gravityToggle)
            {
                Physics2D.gravity = new Vector2(0, 9.81f); // Gravity goes up
                transform.localScale = new Vector3(transform.localScale.x, -1, transform.localScale.z); // Flip scale to face upwards
            }
            else
            {
                Physics2D.gravity = new Vector2(0, -9.81f); // Gravity goes down
                transform.localScale = new Vector3(transform.localScale.x, 1, transform.localScale.z); // Flip scale to face downwards
            }


            StartCoroutine(EndOfAnimation());
        }
        //---------------------------------------------------------------------------------------------------
     
    }

    private IEnumerator EndOfAnimation()
    {
        yield return new WaitForSeconds(0.6f);
        playerAnimatorController.SetBool("isInteracting", false);
    }

    private void PlayerMovement()
    {
        float moveX = 0;
        float moveY = 0;

        // Horizontal movement (common to both in water and on land)
        if (Input.GetKey(KeyCode.A) && isFrozen == false)
        {
            moveX = -1;
        }
        else if (Input.GetKey(KeyCode.D) && isFrozen == false)
        {
            moveX = 1;
        }

        // Vertical movement while in water
        if (isInWater && isWaterWorld == true)
        {
            if (Input.GetKey(KeyCode.W))
            {
                moveY = 1;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                moveY = -1;
            }

            // Adjust player velocity for both horizontal and vertical movement in water
            playersRB.velocity = new Vector2(moveX * waterMoveSpeed, moveY * waterMoveSpeed);
        }

        else if (isInWater && isWaterWorld != true)
        {
            playersRB.velocity = new Vector2(moveX * waterMoveSpeed, moveY * waterMoveSpeed);
        }

        else
        {
            // Normal horizontal movement
            playersRB.velocity = new Vector2(moveX * moveSpeed, playersRB.velocity.y);
        }

        // Update animator if applicable
        playerAnimatorController.SetFloat("PlayerSpeed", Mathf.Abs(moveX));

        // Jumping (common to both in water and on land)
        if (isGrounded && Input.GetKeyDown(KeyCode.Space) && !isFrozen)
        {
            playerAnimatorController.SetBool("isJumping", true);

            // Jump direction based on gravityToggle
            Vector2 jumpDirection = gravityToggle ? Vector2.down : Vector2.up;
            playersRB.AddForce(jumpDirection * jumpForce, ForceMode2D.Impulse);

            // Flip the grounded state after the jump
            isGrounded = false;
        }
    }


    private void OnCollisionEnter2D(Collision2D collision)  
    {
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("MovingPlatform"))
        {
            isGrounded = true;
            playerAnimatorController.SetBool("isJumping", false);
        }
    }

    public override void OnNetworkSpawn()
    {
    
        // Set the initial player data for the local player
        UpdatePlayerVisuals(onlinePlayerData.Value);

        // Subscribe to value changes in the NetworkVariable
        onlinePlayerData.OnValueChanged += (MyTransferrableData previousValue, MyTransferrableData newValue) =>
        {
            UpdatePlayerVisuals(newValue);
        };

        isSpriteFlipped.OnValueChanged += (bool previousValue, bool newValue) =>
        {
            spriteRenderer.flipX = newValue;
            mouseSpriteRenderer.flipX = newValue;
        };

        if (IsOwner && cameraFollow != null)
        {
            cameraFollow.playerTarget = transform;
        }

        if(IsServer)
        {
            UpdateExistingPlayers();
        }


        // Ensure all players are updated for new clients
        

        base.OnNetworkSpawn();
    }


    private void UpdateExistingPlayers()
    {
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject != null && client.PlayerObject != NetworkObject) // Ensure we don't update ourselves
            {
                var otherPlayerController = client.PlayerObject.GetComponent<NewPlayerController>();
                if (otherPlayerController != null)
                {
                    // Trigger the update for this player’s visuals
                    UpdatePlayerVisuals(otherPlayerController.onlinePlayerData.Value);
                }
            }
        }
    }
    private void UpdatePlayerVisuals(MyTransferrableData playerData)
    {
        // Check if this is the local player
        if (IsOwner)
        {
            // Set the local player's color to full opacity
            spriteRenderer.color = new Color(playerData.rValue, playerData.gValue, playerData.bValue, 1f);
            mouseSpriteRenderer.color = spriteRenderer.color;//changing mouse colour
        }
        else
        {
            // Set other players' colors to slightly transparent
            spriteRenderer.color = new Color(playerData.rValue, playerData.gValue, playerData.bValue, 0.6f); // Adjust the alpha for teammates
            mouseSpriteRenderer.color = spriteRenderer.color;//changing mouse colour
        }

        // Update player name
        playerName.text = playerData.playerTag.ToString();
    }
    // Function to be triggered by button press
    public void OnSelectTagAndColor(string newPlayerTag, Color newPlayerColour)
    {
        if (!IsOwner) return;

        MyTransferrableData newData = new MyTransferrableData
        {
            playerTag = new FixedString128Bytes(newPlayerTag),
            rValue = newPlayerColour.r,
            gValue = newPlayerColour.g,
            bValue = newPlayerColour.b,
            aValue = 1f // Set full alpha for the local player
        };

        // Update the NetworkVariable so that all players see the changes
        onlinePlayerData.Value = newData;
        playerName.text = newPlayerTag;
        spriteRenderer.color = newPlayerColour; // Set the local player's color
        mouseSpriteRenderer.color = spriteRenderer.color;
    }

    //look at setting the different interactable inputs depending on player type...

    public void SetPlayerNameTagAndColour(string newPlayerTag, Color newPlayerColour)
    {
        if (!IsOwner) return;

        MyTransferrableData newData = new MyTransferrableData
        {
            playerTag = new FixedString128Bytes(newPlayerTag),
            rValue = newPlayerColour.r,
            gValue = newPlayerColour.g,
            bValue = newPlayerColour.b,
            aValue = newPlayerColour.a
        };

        // Debug log to check color values
        Debug.Log($"Setting color: {newPlayerColour} with tag: {newPlayerTag}");

        // Update the NetworkVariable so that all players see the changes
        onlinePlayerData.Value = newData;
        playerName.text = newPlayerTag;
        spriteRenderer.color = newPlayerColour;
        mouseSpriteRenderer.color = spriteRenderer.color;
        CharacterChecker();//check the player colour after its assigned
    }


    private void CharacterChecker()
    {
        //this checks the colour of the player and triggersthe bool that will determine which wolrd/ background that character should see locally...#
        //checking against each of the possible character colours...
        Color redTargetColour = new Color(1.00f,0.00f,0.00f,1.00f);
        Color blueTargetColour = new Color(0.00f, 0.6156863f,1.00f,1.00f);
        Color greenTargetColour = new Color(0.00f,1.00f,0.00f,1.00f);
        Color purpleTargetColour = new Color(0.8313726f, 0.2705882f,1.00f,1.00f);

        if (IsOwner)
        {

            if (spriteRenderer.color == redTargetColour)
            {
                //set red to true
                isWaterWorld = false;
                isFireWorld = true;
                isPlantWorld = false;
                isMagicWorld = false;
                Debug.Log("the RED world has been selected");

            }

            if (spriteRenderer.color == blueTargetColour)
            {
                //set red to true
                isWaterWorld = true;
                isFireWorld = false;
                isPlantWorld = false;
                isMagicWorld = false;
                Debug.Log("the BLUE world has been selected");
            }

            if (spriteRenderer.color == greenTargetColour)
            {
                //set red to true
                isWaterWorld = false;
                isFireWorld = false;
                isPlantWorld = true;
                isMagicWorld = false;
                Debug.Log("the GREEN world has been selected");
            }

            if (spriteRenderer.color == purpleTargetColour)
            {
                //set red to true
                isWaterWorld = false;
                isFireWorld = false;
                isPlantWorld = false;
                isMagicWorld = true;
                Debug.Log("the PURPLE world has been selected");
            }

        }
    }


    [ServerRpc]
    private void createBulletShotFromClientServerRpc(Vector3 projectilePosition, Quaternion projectileRotation, bool isSpriteFlipped)
    {
        GameObject spawnedObject = Instantiate(spawnedObjectTransform, projectilePosition, projectileRotation);
        spawnedObject.GetComponent<NetworkObject>().Spawn(true);

        // Pass the flip state to the projectile
        ProjectileMovement projectileMovement = spawnedObject.GetComponent<ProjectileMovement>();
        projectileMovement.isSpriteFlipped = isSpriteFlipped;
    }
}
