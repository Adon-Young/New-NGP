using Unity.Netcode;
using UnityEngine;

public class ScoreController : NetworkBehaviour
{
    public NetworkVariable<int> networkMouseOfferings = new NetworkVariable<int>(0);  // Player's mouse score
    public NetworkVariable<int> networkStatueScore = new NetworkVariable<int>(0);  // Player's statue score
    public static NetworkVariable<int> totalMouseOfferings = new NetworkVariable<int>(0);  // Total mouse score for all players
    public static NetworkVariable<int> totalStatueScore = new NetworkVariable<int>(0);  // Total statue score for all players

    // Update network scores (only called on the server)
    public void UpdateNetworkScores(int mouseOfferings, int statueScore)
    {
        if (IsServer)  // Ensure only the server can update network scores
        {
            networkMouseOfferings.Value = mouseOfferings;
            networkStatueScore.Value = statueScore;
        }
    }

    // Local score update (for client-side tracking)
    public void UpdateLocalScores(int mouseOfferings, int statueScore)
    {
        // Local tracking of mouse offerings and statue score
        // This could be used for UI or other local client interactions
        networkMouseOfferings.Value = mouseOfferings; // Example, you can track local scores as needed
        networkStatueScore.Value = statueScore;
    }
}
