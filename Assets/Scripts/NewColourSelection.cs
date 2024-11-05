using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NewColourSelection : MonoBehaviour
{
    [SerializeField] private string playerTag; // e.g., "Fire", "Ice", "Plant", "Magic"
    [SerializeField] private Color playerColor; // Corresponding color for this character
    private NewPlayerController playerController;
    private LevelController levelController;//reference to the level controller script
    private void Start()
    {
        levelController = FindObjectOfType<LevelController>();
        if (levelController == null)
        {
            Debug.LogError("LevelController not found in the scene!");
        }
        // Ensure the button has a listener to call the SelectCharacter function when pressed
        GetComponent<Button>().onClick.AddListener(SelectCharacter);
    }

    private void SelectCharacter()
    {
        // Find the local player (make sure the script has a way to reference the player's controller)
        if (NetworkManager.Singleton.LocalClient.PlayerObject != null)
        {
            playerController = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<NewPlayerController>();

            // Check if we found the local player's controller
            if (playerController != null)
            { 
                
                playerController.SetPlayerNameTagAndColour(playerTag, playerColor);

                playerController.isFrozen = false;


                if (levelController != null)
                {
                    levelController.WorldChecker(playerController);
                }
               
               
            }
        }
    }
}
