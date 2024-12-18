using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class EndOfGame : NetworkBehaviour
{
    public LevelTimer levelTimer; // Reference to the LevelTimer script
    public GameObject LearerboardScreen;
    public GameObject playerHUD;
    public GameObject winText; // UI element for the win message
    public GameObject loseText; // UI element for the lose message
    public AudioSource endLevelAudio; // Audio source for the end level sound

    // Network variables to sync across clients
    public  NetworkVariable<bool> gameEnded = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public  NetworkVariable<int> gameResult = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);




    private void Update()
    {
        if (levelTimer == null)
        {
            return;
        }

        // Check for game end conditions (win)
        if (!gameEnded.Value && ScoreController.totalMouseOfferings.Value == 4 && ScoreController.totalStatueScore.Value == 4)
        {
            if (IsServer) // Only the server can stop the timer and set the game end
            {
                Debug.Log("Win condition met: Stopping timer and ending game.");
                levelTimer.StopTimerServerRpc();
                gameEnded.Value = true; // Sync game end across all clients
                gameResult.Value = 1; // 1 = win
                StartCoroutine(EndOfLevel());

                // Call the serverRpc to notify clients about the win
                DisplayResultOnAllClientsServerRpc(); // <-- This is the missing call
            }
        }

        // Check for loss condition (any cat health at 0)
        if (!gameEnded.Value && IsServer && AnyCatHealthZero()) // Only the server checks this
        {
            Debug.Log("Loss condition met: Stopping timer and ending game.");
            levelTimer.StopTimerServerRpc();
            gameEnded.Value = true;
            gameResult.Value = -1; // -1 = lose
     
            // Directly display the leaderboard for loss
            DisplayLeaderboardClientRpc();

            // Call the serverRpc to notify clients about the loss
            DisplayResultOnAllClientsServerRpc(); // <-- This is the missing call
        }
    }

    // Function to dynamically check all CatHealth scripts for 0 health
    private bool AnyCatHealthZero()
    {
        // Find all active CatHealth components in the scene
        CatHealth[] allCats = FindObjectsOfType<CatHealth>();

        // Loop through each CatHealth instance to check health
        foreach (CatHealth cat in allCats)
        {
            if (cat.currentCatHealth.Value <= 0) // If any cat has 0 health
            {
                Debug.Log("A cat's health reached zero, triggering loss condition.");
                return true; // Trigger loss condition
            }
        }
        return false; // No cats with 0 health
    }

    private IEnumerator EndOfLevel()
    {
        // Play audio on all clients
        PlayEndLevelAudioClientRpc();

        // Wait for the audio to finish (adjust the duration to match your audio length)
        yield return new WaitForSeconds(endLevelAudio.clip.length);
   
        // Show leaderboard/GameOverScreen
        Debug.Log("Ending level and displaying leaderboard.");
        DisplayLeaderboardClientRpc();

     
    }

    // ClientRpc to synchronize audio playback
    [ClientRpc]
    public void PlayEndLevelAudioClientRpc()
    {
        if (endLevelAudio != null && !endLevelAudio.isPlaying)
        {
            Debug.Log("Playing end level audio.");
            endLevelAudio.Play();
        }
    }

    // ClientRpc to update the UI for all clients
    [ClientRpc]
    public void DisplayLeaderboardClientRpc()
    {
        // This method will be called on all clients to display the leaderboard
        Debug.Log("Displaying leaderboard to all clients.");
        NewPlayerController.FreezePlayer();

        // Hide the HUD and show the leaderboard
        playerHUD.SetActive(false);
        LearerboardScreen.SetActive(true);
    }

    // ServerRpc to notify all clients to display the appropriate win/lose text
    [ServerRpc]
    public void DisplayResultOnAllClientsServerRpc()
    {
        Debug.Log("Sending result to all clients (win/lose).");
        // Call the client method to update the UI for all players
        DisplayResultOnAllClientsClientRpc(gameResult.Value); // <-- This sends the result to the clients
    }

    // ClientRpc to display the win or lose text on all clients
    [ClientRpc]
    public void DisplayResultOnAllClientsClientRpc(int result)
    {
        // Update win/lose text based on the game result
        Debug.Log("Displaying result to all clients: " + (result == 1 ? "Win" : "Lose"));
        if (result == 1) // Win
        {
            winText.SetActive(true);
            loseText.SetActive(false);
        }
        else if (result == -1) // Lose
        {
            winText.SetActive(false);
            loseText.SetActive(true);

        }
    }
}
