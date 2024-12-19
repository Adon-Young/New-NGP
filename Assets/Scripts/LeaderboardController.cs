using Unity.Netcode;
using UnityEngine;
using TMPro;

public class LeaderboardManager : NetworkBehaviour
{
    public TMP_Text FireHealth;
    public TMP_Text WaterHealth;
    public TMP_Text PlantHealth;
    public TMP_Text MagicHealth;

    float waterCatHealth;
    float fireCatHealth;
    float plantCatHealth;
    float magicCatHealth;

    void Update()
    {
        // Only the server processes leaderboard updates
        if (!IsServer) return;

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        Debug.Log($"Number of players found: {players.Length}");

        foreach (GameObject player in players)
        {
            PlayerCollision playerCollision = player.GetComponent<PlayerCollision>();
            CatHealth catHealth = player.GetComponent<CatHealth>();

            Debug.Log($"Checking player: {player.name}, has PlayerCollision: {playerCollision != null}, has CatHealth: {catHealth != null}");

            if (playerCollision != null && catHealth != null)
            {
                // Accessing the PlayerType via the NetworkVariable.Value
                Debug.Log($"Player {player.name} has PlayerType: {playerCollision.playerType.Value}, Enum Value: {(int)playerCollision.playerType.Value}");

                // Switch based on the synced PlayerType (using .Value to get the actual networked value)
                switch (playerCollision.playerType.Value)
                {
                    case PlayerCollision.PlayerType.Water:
                        waterCatHealth = catHealth.currentCatHealth.Value;
                        break;
                    case PlayerCollision.PlayerType.Fire:
                        fireCatHealth = catHealth.currentCatHealth.Value;
                        break;
                    case PlayerCollision.PlayerType.Plant:
                        plantCatHealth = catHealth.currentCatHealth.Value;
                        break;
                    case PlayerCollision.PlayerType.Magic:
                        magicCatHealth = catHealth.currentCatHealth.Value;
                        break;
                }
            }
            else
            {
                // Log if we couldn't find PlayerCollision or CatHealth for a player
                if (playerCollision == null)
                    Debug.LogWarning($"Player {player.name} is missing PlayerCollision.");
                if (catHealth == null)
                    Debug.LogWarning($"Player {player.name} is missing CatHealth.");
            }
        }

        // Update UI for all clients
        UpdateLeaderboardUIClientRpc(waterCatHealth, fireCatHealth, plantCatHealth, magicCatHealth);
    }

    [ClientRpc]
    private void UpdateLeaderboardUIClientRpc(float waterHealth, float fireHealth, float plantHealth, float magicHealth)
    {
        // Ensure the values are not null or incorrect before using them
        Debug.Log($"Water Health: {waterHealth}, Fire Health: {fireHealth}, Plant Health: {plantHealth}, Magic Health: {magicHealth}");

        // Check if any health value is invalid
        if (float.IsNaN(waterHealth) || float.IsNaN(fireHealth) || float.IsNaN(plantHealth) || float.IsNaN(magicHealth))
        {
            Debug.LogError("One of the health values is invalid (NaN).");
        }

        // Update UI for all clients
        WaterHealth.text = $"Water: {waterHealth}";
        FireHealth.text = $"Fire: {fireHealth}";
        PlantHealth.text = $"Plant: {plantHealth}";
        MagicHealth.text = $"Magic: {magicHealth}";

        Debug.Log($"Updated leaderboard: Water {waterHealth}, Fire {fireHealth}, Plant {plantHealth}, Magic {magicHealth}");
    }
}
