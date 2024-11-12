using UnityEngine;

public class Water : MonoBehaviour
{
    private bool isRising = false; // State of whether the water is rising
    private float riseSpeed = 0.5f; // Speed at which the water rises
    private NewPlayerController playerController;
    private void OnTriggerStay2D(Collider2D other)
    {
        // Check if the object we are overlapping with has the Plantform script
        Plantform plant = other.GetComponent<Plantform>();
        if (plant != null && plant.IsSeedling) // Only interact if it's in seedling state
        {
            // Call the method on the plant to change its state
            plant.GrowPlant();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // If the player enters the water, get the player controller
        if (other.CompareTag("Player"))
        {
            playerController = other.GetComponent<NewPlayerController>();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // If the player exits the water, clear the player controller
        if (other.CompareTag("Player"))
        {
            playerController = null;
        }
    }
    private void Update()
    {
        // Ensure playerController is valid and player is in water world and in water
        if (playerController != null && playerController.isWaterWorld && playerController.isInWater)
        {
            // Check if mouse button is pressed, toggle water rising state
            if (Input.GetMouseButtonDown(0))
            {
                ToggleRising();
            }

            // While the player is in the water, raise the water if the rising state is true
            if (isRising)
            {
                transform.position += new Vector3(0, riseSpeed * Time.deltaTime, 0); // Raise the water
            }
        }


    }

    private void ToggleRising()
    {
        isRising = !isRising;
    }
}


