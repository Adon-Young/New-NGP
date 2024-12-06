using UnityEngine;
using Unity.Netcode;

public class EndOfGame : MonoBehaviour
{
    public LevelTimer levelTimer; // Reference to the LevelTimer script

    private bool gameEnded = false;

    private void Update()
    {
        if (levelTimer == null)
        {
            Debug.LogError("Reference for LevelTimer is missing!");
            return;
        }

        if (!gameEnded && PlayerCollision.totalMouseOfferings.Value == 4 && PlayerCollision.totalStatueScore.Value == 4)
        {
            if (levelTimer.IsServer)
            {
                levelTimer.StopTimerServerRpc();
                Debug.Log("Game timer stopped! Conditions met.");
                gameEnded = true; // Prevent further calls
            }
            else
            {
                Debug.LogError("You are not the server! Timer can only be stopped by the server.");
            }
        }
    }
}
