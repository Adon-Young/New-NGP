using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // Add this to use TextMeshPro
using System.Linq;
using System.Collections.Generic;

public class MultiPlayerManager : NetworkBehaviour // Inherit from NetworkBehaviour
{
    private Dictionary<ulong, GameObject> playerObjects = new Dictionary<ulong, GameObject>();
    [SerializeField] private Button hostButton;
    [SerializeField] private Button serverButton;
    [SerializeField] private Button clientButton;

    // Variable to track the maximum number of players
    public int maximumPlayerCount = 4; // capping it at 4 players per server

    // Network variable to hold the current player count (synced across the network)
    public NetworkVariable<int> currentPlayerCount = new NetworkVariable<int>(0);

    // Reference to the spawn point script
    private SpawnPoints spawnPointScript;

    // New TMP_Text field to display current player count
    [SerializeField] private TMP_Text playerCountText; // Reference to the TextMeshPro UI text element

    private void Awake()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected; // adding to the dictionary
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected; // adding the disconnect callback

        // Set up the on-click events using delegates
        hostButton.onClick.AddListener(() =>
        {
            Debug.Log("Hosting game...");
            NetworkManager.Singleton.StartHost();
        });

        serverButton.onClick.AddListener(() =>
        {
            Debug.Log("Starting server...");
            NetworkManager.Singleton.StartServer();
        });

        clientButton.onClick.AddListener(() =>
        {
            Debug.Log("Joining game as client...");
            NetworkManager.Singleton.StartClient();
        });
    }

    private void OnDestroy()
    {
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected; // cleaning up callbacks
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected; // cleaning up callbacks
    }

    private void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += (id) =>
        {
            Debug.Log($"Client connected successfully with ID: {id}"); // tells us who connected
        };

        NetworkManager.Singleton.OnClientDisconnectCallback += (id) =>
        {
            Debug.Log($"Client disconnected with ID: {id}"); // tells us who disconnected
        };

        NetworkManager.Singleton.OnServerStarted += () =>
        {
            Debug.Log("Server started!"); // tells us the server has been created/ up and running
        };

        // Initialize the player count text display at the start
        UpdatePlayerCountText();

        // Listen for changes in the currentPlayerCount NetworkVariable
        currentPlayerCount.OnValueChanged += OnPlayerCountChanged;
    }

    private void OnClientConnected(ulong clientId)
    {
        if (NetworkManager.Singleton.ConnectedClients.Count > maximumPlayerCount)
        {
            NetworkManager.Singleton.DisconnectClient(clientId); // disconnect the client trying to connect to the server as its full!
            Debug.Log($"Client {clientId} has disconnected");
        }
        else
        {
            // Update the player count on the server side
            if (IsServer)
            {
                currentPlayerCount.Value = GetCurrentPlayerCount(); // Set the network variable to the current count
            }

            SpawnPlayer(clientId);
        }
    }

    public int GetCurrentPlayerCount()
    {
        return NetworkManager.Singleton.ConnectedClients.Count;
    }

    private void SpawnPlayer(ulong clientId)
    {
        if (!IsServer) return; // Only the server should handle spawning

        Transform spawnPoint = spawnPointScript.GetAvailableSpawnPoint(); // Get an available spawn point
        if (spawnPoint == null)
        {
            Debug.LogError("No available spawn points!"); // Handle no available spawn points
            return;
        }

        // Instantiate the player prefab (make sure you assign this in the Inspector)
        var playerPrefab = Resources.Load<GameObject>("PlayerPrefab"); // Load your player prefab from Resources
        var playerObject = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);

        // Spawn the player in the network
        playerObject.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);

        // Store the player object in the dictionary
        playerObjects[clientId] = playerObject;
    }

    // New method to update the displayed player count in the UI
    private void UpdatePlayerCountText()
    {
        if (playerCountText != null)
        {
            playerCountText.text = $"Players Connected: {currentPlayerCount.Value} / {maximumPlayerCount}";
        }
    }

    // Callback method for when the NetworkVariable value changes
    private void OnPlayerCountChanged(int previousValue, int newValue)
    {
        UpdatePlayerCountText(); // Update the displayed player count when the value changes
    }

    // Public function to allow players to leave the server (disconnection logic)
    public void LeaveGame()
    {
        // Check if the player is the host
        if (IsHost)
        {
            // If the host is leaving, find another client to become the new host
            Debug.Log("Host is leaving the game, attempting to transfer host role...");

            // If there are other clients connected, choose the first one as the new host
            if (NetworkManager.Singleton.ConnectedClients.Count > 1)
            {
                // Select the first client who is not the current host
                ulong newHostClientId = NetworkManager.Singleton.ConnectedClients
                    .Where(client => client.Key != NetworkManager.Singleton.LocalClientId)  // Exclude the current host
                    .Select(client => client.Key) // Get the client id (ulong)
                    .FirstOrDefault(); // Get the first available client

                if (newHostClientId != default)
                {
                    // Log and set the new host.
                    Debug.Log($"New host assigned: Client {newHostClientId}");

                    // Optionally: Send a network message to the new client that they're the host.
                    // For example, you can use a custom network message or method to notify the new host.

                    // Restart the server and make the new client the host (this would need to be implemented manually)
                    NetworkManager.Singleton.Shutdown(); // Shut down the current server

                    // Restart the server and reassign the role
                    NetworkManager.Singleton.StartHost(); // Restart server as the new host
                }
            }
            else
            {
                // If there are no clients left, shut down the server
                Debug.Log("No clients left, shutting down the server...");
                NetworkManager.Singleton.Shutdown();
            }
        }
        else if (IsClient)
        {
            // If it's a client, simply disconnect
            Debug.Log("Client is leaving the game...");
            NetworkManager.Singleton.Shutdown(); // Disconnect the client
        }
    }



    // Callback for when a player disconnects
    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"Client {clientId} disconnected");

        if (IsServer)
        {
            // Update player count when a player leaves
            currentPlayerCount.Value = GetCurrentPlayerCount(); // Update the network variable for all clients

            // If the player object exists, despawn it
            if (playerObjects.ContainsKey(clientId))
            {
                Destroy(playerObjects[clientId]); // Destroy the player object
                playerObjects.Remove(clientId); // Remove the reference from the dictionary
            }
        }
    }
}