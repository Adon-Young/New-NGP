using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BeginGame : NetworkBehaviour
{
    public NetworkVariable<int> characterSelected = new NetworkVariable<int>(0);  // Score for Mouse Offerings
    private int numberOfCharactersSelected = 0;  // Local score for Mouse Offerings
    LevelTimer gamesLevelTimerReference;
    public GameObject charSelectionScreen;
    public CatHealth cateHealthScript;
    private bool hasTeleported = false;

    // Start is called before the first frame update
    private void Awake()
    {
        if (gamesLevelTimerReference == null)
        {
            gamesLevelTimerReference = FindObjectOfType<LevelTimer>();
        }
    }

    public void Update()
    {
        OnAllPlayersSelected();
    }



    [ServerRpc(RequireOwnership = false)]
    public void UpdateCharacterSelectionServerRpc(int addValue, ulong clientId)
    {
        if (characterSelected.Value < 4) // Ensure only up to 4 characters can be selected
        {
            characterSelected.Value += addValue; // Increment the network variable

            UpdateCharacterSelectionClientRpc(characterSelected.Value); // Notify all clients
        }
        else
        {
 
        }
    }

    [ClientRpc]
    private void UpdateCharacterSelectionClientRpc(int newScore)
    {
        numberOfCharactersSelected = newScore; // Update the local value
    }

    public void OnCharacterButtonClicked()
    {
        if (IsClient && !HasAlreadySelected()) // Ensure client is valid and hasn't selected yet
        {
            UpdateCharacterSelectionServerRpc(1, NetworkManager.Singleton.LocalClientId); // Notify the server
        }

    }
    //players shouldnt be able to select more than oonce as ive added in the blockers for the buttoons but just in case...
    private bool HasAlreadySelected()
    {
        return numberOfCharactersSelected >= 4; // Prevent extra selections after limit
    }

    private void OnAllPlayersSelected()
    {
        // Check if the game is already over to prevent unintended restarts
        if (gamesLevelTimerReference != null && gamesLevelTimerReference.IsGameOver)
        {
            return;
        }

        if (characterSelected.Value == 4 && !EndOfGame.gameEnded.Value) // Make sure game hasn't ended
        {
            // Transition UI
            charSelectionScreen.SetActive(false);
            NewPlayerController.UnfreezePlayer();

            // Start game countdown timer
            if (gamesLevelTimerReference != null)
            {
                gamesLevelTimerReference.StartTimerServerRpc(); // Trigger the countdown and timer start
            }

            // Teleport all players to their spawn points only once
            if (!hasTeleported)
            {
                hasTeleported = true; // Set the flag to true to prevent future teleports
                TeleportAllPlayersToSpawnPoints();
            }
        }
    }

    private void TeleportAllPlayersToSpawnPoints()
    {
        foreach (var player in FindObjectsOfType<CatHealth>())
        {
            player.TeleportToSafeZone();
        }
    }

}
