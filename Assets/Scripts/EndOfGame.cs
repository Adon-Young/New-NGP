using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class EndOfGame : NetworkBehaviour
{
    public LevelTimer levelTimer; // Reference to the LevelTimer script
    public GameObject LearerboardScreen;
    public GameObject playerHUD;
    public AudioSource endLevelAudio; // Audio source for the end level sound

    // Change to a NetworkVariable to sync across all clients
    public static NetworkVariable<bool> gameEnded = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private void Update()
    {
        if (levelTimer == null)
        {
            return;
        }

        // Check for game end conditions
        if (!gameEnded.Value && ScoreController.totalMouseOfferings.Value == 4 && ScoreController.totalStatueScore.Value == 4)
        {
            if (IsServer) // Only the server can stop the timer and set the game end
            {
                levelTimer.StopTimerServerRpc(); // Stop the timer on the server

                // Update gameEnded on the server and trigger the update for clients
                gameEnded.Value = true; // Sync game end across all clients

                // Start the end sequence coroutine
                StartCoroutine(EndOfLevel());
            }
        }
    }

    private IEnumerator EndOfLevel()
    {
        // Play audio on all clients
        PlayEndLevelAudioClientRpc();

        // Wait for the audio to finish (adjust the duration to match your audio length)
        yield return new WaitForSeconds(endLevelAudio.clip.length);

        // Show leaderboard/GameOverScreen
        DisplayLeaderboardClientRpc();
    }

    // ClientRpc to synchronize audio playback
    [ClientRpc]
    public void PlayEndLevelAudioClientRpc()
    {
        if (endLevelAudio != null && !endLevelAudio.isPlaying)
        {
            endLevelAudio.Play();
        }
    }

    // ClientRpc to update the UI for all clients
    [ClientRpc]
    public void DisplayLeaderboardClientRpc()
    {
        // This method will be called on all clients to display the leaderboard
        // Freeze player actions on all clients
        NewPlayerController.FreezePlayer();

        // Hide the HUD and show the leaderboard after freezing
        playerHUD.SetActive(false); // Hide the HUD
        LearerboardScreen.SetActive(true); // Show the leaderboard screen
    }
}
