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
        // Check if the object entering the trigger is the player
        if (other.CompareTag("Player"))
        {
            // Get the player controller to check conditions
            playerController = other.GetComponent<NewPlayerController>();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // If the player exits, clear the reference to stop interactions
        if (other.CompareTag("Player"))
        {
            playerController = null;
        }
    }

    private void Update()
    {
        // If water is rising, move it up
        if (isRising)
        {
            transform.position += new Vector3(0, riseSpeed * Time.deltaTime, 0); // Raise the water
        }

        // Check if the player is in the water, and the conditions are met
        if (playerController != null && playerController.isWaterWorld && playerController.isInWater)
        {
            // If the mouse button is pressed, toggle the rising state
            if (Input.GetMouseButtonDown(0))
            {
                ToggleRising();
            }
        }
    }

    // Toggles the water rising state
    private void ToggleRising()
    {
        isRising = !isRising;
    }
}


