using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NewColourSelection : NetworkBehaviour
{
    [SerializeField] private string playerTag; // e.g., "Fire", "Ice", "Plant", "Magic"
    [SerializeField] private Color playerColor; // Corresponding color for this character
    private NewPlayerController playerController;
    private LevelController levelController; // Reference to the level controller script

    [SerializeField] private Text readyText; // Reference to the "Ready!" text
    private static int playersReadyCount = 0; // Static variable to track how many players are ready
    private static readonly int maxPlayers = 4; // Maximum number of players (adjust this as needed)

    [SerializeField] private Image[] blockerImages; // Array to hold the blocker UI images for each character

    // The function that is called when a player selects their character
    private void Start()
    {
        levelController = FindObjectOfType<LevelController>();
        if (levelController == null)
        {
            Debug.LogError("LevelController not found in the scene!");
        }

        // Initially disable all blocker images
        foreach (Image blockerImage in blockerImages)
        {
            blockerImage.gameObject.SetActive(false);
        }

        // Ensure the button has a listener to call the SelectCharacter function when pressed
        GetComponent<Button>().onClick.AddListener(SelectCharacter);
    }

    // This is where the character selection happens
    private void SelectCharacter()
    {
        if (NetworkManager.Singleton.LocalClient.PlayerObject != null)
        {
            playerController = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<NewPlayerController>();

            if (playerController != null)
            {
                playerController.SetPlayerNameTagAndColour(playerTag, playerColor);
                playerController.isFrozen = false;

                // Set player as ready
                playersReadyCount++;

                // Update the "Ready!" text
                if (readyText != null)
                {
                    readyText.text = "READY!";
                }

                // Disable the button after player has pressed it
                GetComponent<Button>().interactable = false;

                // Block other players from selecting this character by enabling the UI blocker image
                int characterIndex = GetCharacterIndex(playerTag); // Assuming you have a way to match the character with the index
                if (characterIndex >= 0 && characterIndex < blockerImages.Length)
                {
                    // Use Invoke to delay the blocker image enabling
                    Invoke(nameof(EnableBlockerImages), 0.1f);
                }

                if (levelController != null)
                {
                    levelController.WorldChecker(playerController);
                }

                // Check if all players are ready
                if (playersReadyCount == maxPlayers)
                {
                    StartGame(); // Call the start game function when all players are ready
                }
            }
        }
    }

    // Helper function to get the index of the character
    private int GetCharacterIndex(string tag)
    {
        switch (tag)
        {
            case "Water": return 0;
            case "Plant": return 1;
            case "Magic": return 2;
            case "Fire": return 3;
            default: return -1;
        }
    }

    private void EnableBlockerImages()
    {
        int characterIndex = GetCharacterIndex(playerTag); // Get the current player's character index
        if (characterIndex >= 0 && characterIndex < blockerImages.Length)
        {
            EnableBlockerImageForAll(characterIndex);
        }
    }

    // Function to enable the blocker UI images for all players (this function is executed on all clients)
    private void EnableBlockerImageForAll(int index)
    {
        Debug.Log("Attempting to enable blocker image at index: " + index);
        if (IsServer) // Ensure this is done on the server side
        {
            // Sync the blocker image state across all clients
            EnableBlockerImageClientRpc(index);
        }
    }

    [ClientRpc]
    private void EnableBlockerImageClientRpc(int index)
    {
        Debug.Log("Enabling blocker image on all clients for index: " + index);
        if (index >= 0 && index < blockerImages.Length)
        {
            blockerImages[index].gameObject.SetActive(true); // Enable the image for the selected character
        }
    }

    // Function to start the game (this can be expanded later with actual game start logic)
    private void StartGame()
    {
        // Logic for starting the game goes here
        Debug.Log("All players are ready. Starting the game...");
    }

    // Networked variable to track the readiness state (if necessary, for syncing across clients)
    private NetworkVariable<bool> isReady = new NetworkVariable<bool>(false);

    // Update the ready state for the current player
    public void UpdateReadyState(bool ready)
    {
        isReady.Value = ready;

        // Inform all clients about this player's readiness status
        if (IsServer)
        {
            CheckAllPlayersReady();
        }
    }

    // Server-side function to check if all players are ready
    private void CheckAllPlayersReady()
    {
        bool allReady = true;
        foreach (var client in NetworkManager.Singleton.ConnectedClients)
        {
            if (!client.Value.PlayerObject.GetComponent<NewColourSelection>().isReady.Value)
            {
                allReady = false;
                break;
            }
        }

        if (allReady)
        {
            // Call the function to start the game
            StartGame();
        }
    }

    // This method will be called when a player leaves (or despawns)
    private void OnNetworkDespawn()
    {
        // Reset UI for all clients when a player leaves
        if (IsServer)
        {
            ResetUIForAllClients();
        }
    }

    // Function to reset the button, blocker image, and READY text for all clients
    private void ResetUIForAllClients()
    {
        if (IsServer)
        {
            // Reset the "READY!" text
            readyText.text = "";

            // Disable the blocker images for everyone
            foreach (Image blockerImage in blockerImages)
            {
                blockerImage.gameObject.SetActive(false);
            }

            // Reset the button state for everyone (enable it)
            GetComponent<Button>().interactable = true;

            // Sync this across all clients
            ResetUIClientRpc();
        }
    }

    [ClientRpc]
    private void ResetUIClientRpc()
    {
        // Reset the "READY!" text and blocker images for all clients
        readyText.text = "";
        foreach (Image blockerImage in blockerImages)
        {
            blockerImage.gameObject.SetActive(false);
        }

        // Enable the button
        GetComponent<Button>().interactable = true;
    }
}
