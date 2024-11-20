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
    [SerializeField] private Button[] characterButtons; // Array to hold character selection buttons

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

                // Disable this button locally to prevent the player from selecting another character
                GetComponent<Button>().interactable = false;

                // Block all other character buttons locally for this player
                DisableOtherButtonsLocally();

                // Notify the server to block the character for all players
                int characterIndex = GetCharacterIndex(playerTag);
                if (characterIndex >= 0 && characterIndex < blockerImages.Length)
                {
                    RequestBlockerActivationServerRpc(characterIndex);
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

    // Disables all character buttons except the one the player selected
    private void DisableOtherButtonsLocally()
    {
        foreach (Button button in characterButtons)
        {
            if (button != GetComponent<Button>())
            {
                button.interactable = false;
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

    // ServerRpc to handle blocking character selection for all players
    [ServerRpc(RequireOwnership = false)]
    private void RequestBlockerActivationServerRpc(int characterIndex)
    {
        EnableBlockerImageClientRpc(characterIndex);
    }

    // ClientRpc to enable the blocker UI for the specified character index
    [ClientRpc]
    private void EnableBlockerImageClientRpc(int characterIndex)
    {
        if (characterIndex >= 0 && characterIndex < blockerImages.Length)
        {
            blockerImages[characterIndex].gameObject.SetActive(true);
        }
    }

    // Function to start the game (this can be expanded later with actual game start logic)
    private void StartGame()
    {
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
            StartGame();
        }
    }

    // This method will be called when a player leaves (or despawns)
    private void OnNetworkDespawn()
    {
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
            readyText.text = "";

            foreach (Image blockerImage in blockerImages)
            {
                blockerImage.gameObject.SetActive(false);
            }

            GetComponent<Button>().interactable = true;
            ResetUIClientRpc();
        }
    }

    [ClientRpc]
    private void ResetUIClientRpc()
    {
        readyText.text = "";
        foreach (Image blockerImage in blockerImages)
        {
            blockerImage.gameObject.SetActive(false);
        }
        GetComponent<Button>().interactable = true;
    }
}
