using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // Add this to use TextMeshPro
using System.Linq;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using static LevelTimer;
using static NewPlayerController;
using Unity.Collections;

public class MultiPlayerManager : NetworkBehaviour // Inherit from NetworkBehaviour
{
    public Water waterIsRisingScript;
    public PlayerCollision playerCollisionScript;
  
    public LevelTimer levelTimerScript;
    public EndOfGame endOfGameScript;
    public BeginGame beginGameScript;


    private Dictionary<ulong, GameObject> playerObjects = new Dictionary<ulong, GameObject>();
    [SerializeField] private Button hostButton;
    [SerializeField] private Button serverButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private Button quitButton;

    // Variable to track the maximum number of players
    public int maximumPlayerCount = 4; // capping it at 4 players per server
    // Network variable to hold the current player count (synced across the network)
    public NetworkVariable<int> currentPlayerCount = new NetworkVariable<int>(0);
    // New TMP_Text field to display current player count
    [SerializeField] private TMP_Text playerCountText; // Reference to the TextMeshPro UI text element


    private void Awake()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected; // adding to the dictionary
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected; // adding the disconnect callback

        // Set up the on-click events using delegates
        hostButton.onClick.AddListener(() =>
        {

            NetworkManager.Singleton.StartHost();
            EnableQuitButtonForHost();//so only the host can end the game!
        });

        serverButton.onClick.AddListener(() =>
        {

            NetworkManager.Singleton.StartServer();
        });

        clientButton.onClick.AddListener(() =>
        {

            NetworkManager.Singleton.StartClient();
        });
    }

    private void EnableQuitButtonForHost()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            quitButton.gameObject.SetActive(true); // Enable the Quit button

        }
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


        }


    }

    public int GetCurrentPlayerCount()
    {
        return NetworkManager.Singleton.ConnectedClients.Count;
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
            // If the host is leaving, simply shut down the server
            NetworkManager.Singleton.Shutdown(); // Shut down the server
        }
        else if (IsClient)
        {
            // If it's a client, simply disconnect
            NetworkManager.Singleton.Shutdown(); // Disconnect the client
        }
    }

    // Callback for when a player disconnects
    private void OnClientDisconnected(ulong clientId)
    {
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








    //resetting the game at the end of the level...

    public void DisconnectAndReload()
    {
        // Disconnect all players including the host
        NetworkManager.Singleton.Shutdown();

        // Notify all clients to reload the scene
        if (NetworkManager.Singleton.IsHost)
        {
            // Host reloads the scene locally
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            ResetNetworkVariables();

            // Notify all clients to reload their scene as well
            ReloadSceneForClientsClientRPC();
        }
    }

    // This will notify all clients to reload the scene
    [ClientRpc]
    private void ReloadSceneForClientsClientRPC()
    {
        // Reload the scene for all clients
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        ResetNetworkVariables();
    }

    private void ResetNetworkVariables()
    {
        currentPlayerCount.Value = 0;
      
        waterIsRisingScript.isRising.Value = false;
        EndOfGame.gameEnded.Value = false;
        beginGameScript.characterSelected.Value = 0;
        levelTimerScript.countdownValue.Value = 3;
        levelTimerScript.onlineScoreData.Value = new MyScoreMechanics
        {
            levelScore_score = 0,
            endOfLevel_levelComplete = false,
            endOfCounttDownTimer_timerRunning = false
        };

    }
}







