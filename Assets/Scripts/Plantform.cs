using UnityEngine;

public class Plantform : MonoBehaviour
{
    public Sprite seedlingSprout; // New sprite for seedling state
    public Sprite grownPlant; // Sprite for the grown plant state
    public BoxCollider2D platformCollider; // Reference to BoxCollider2D component
    NewPlayerController playerController;
    private SpriteRenderer spriteRenderer;
    private bool isPlayerTouching = false; // Track if the player is touching the platform
    private bool isSeedling = false; // Keep track if the plant is a seedling

    public bool IsSeedling => isSeedling; // Public property for other scripts to access

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        platformCollider = GetComponent<BoxCollider2D>();
        

    }

    void Update()
    {
        // Check if the player is touching and the mouse button is pressed
        if (isPlayerTouching && Input.GetMouseButtonDown(0))
        {
            ChangeToSeedling();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        playerController = other.GetComponent<NewPlayerController>();

        // Check if the player touches the platform
        if (other.CompareTag("Player") && playerController.isPlantWorld == true)
        {
            isPlayerTouching = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        playerController = other.GetComponent<NewPlayerController>();
        if (other.CompareTag("Player") && playerController.isPlantWorld == true)
        {
            isPlayerTouching = false;
        }
    }

    // Method to change to seedling sprite
    public void ChangeToSeedling()
    {
        if (spriteRenderer != null && seedlingSprout != null)
        {
            spriteRenderer.sprite = seedlingSprout;
            isSeedling = true;
        }
    }

    // Method to change to grown plant state (called by WaterInteraction)
    public void GrowPlant()
    {
        if (isSeedling) // Only grow if it's a seedling
        {
            spriteRenderer.sprite = grownPlant;
            gameObject.tag = "Ground";
            platformCollider.isTrigger = false; // Disable trigger
            platformCollider.size = new Vector2(1.181879f, 0.0925281f); // Adjust collider size
            platformCollider.offset = new Vector2(0.004493207f, 0.3842468f); // Adjust collider offset
        }
    }
}
