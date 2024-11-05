using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MultiPlayerManager : NetworkBehaviour // Inherit from NetworkBehaviour
{
    [SerializeField] private Button hostButton;
    [SerializeField] private Button serverButton;
    [SerializeField] private Button clientButton;
    //this is where ill manage the number of player in and out of the game...

    public int maximumPlayerCount = 4;//capping it at 4 players per server
    public int currentPlayerCount;
    private SpawnPoints spawnPointScript;


    private void Awake()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;//adding to the dictionary

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
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;//taking from the dictionary
    }

    private void Start()
    {

        NetworkManager.Singleton.OnClientConnectedCallback += (id) =>
        {
            Debug.Log($"Client connected successfully with ID: {id}");//tells us who connected
        };

        NetworkManager.Singleton.OnClientDisconnectCallback += (id) =>
        {
            Debug.Log($"Client disconnected with ID: {id}");//tells us who disconnected
        };

        NetworkManager.Singleton.OnServerStarted += () =>
        {
            Debug.Log("Server started!");//tells us the server has been created/ up and running
        };
    }



    private void OnClientConnected(ulong clientId)
    {
        if(NetworkManager.Singleton.ConnectedClients.Count > maximumPlayerCount)
        {
            NetworkManager.Singleton.DisconnectClient(clientId);//disconnect the client trying to connect to the server as its full!
            Debug.Log($"Client {clientId} has disconnected");
        }
        else
        {
            currentPlayerCount = GetCurrentPlayerCount();
            Debug.Log(currentPlayerCount);
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
    }

}
