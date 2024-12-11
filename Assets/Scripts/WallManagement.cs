using Unity.Netcode;
using UnityEngine;

public class WallManagement : NetworkBehaviour
{
    public static WallManagement Instance; // Singleton for easy access

    [Header("Wall References")]
    // Expose these in the inspector so they can be assigned manually
    public GameObject floatingWall;


    private void Awake()
    {
        // Set up the singleton instance
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Ensure the walls are inactive at the start
        if (floatingWall != null)
        {
            floatingWall.SetActive(false); // Ensure FloatingWall is inactive at the start
        }
  

    }

    // ServerRpc to handle wall activation
    public void ActivateFloatingWall()
    {
        if (IsServer)
        {
            // Call the server-side RPC to activate the floating wall
            SetFloatingWallActiveClientRpc(true);
        }
        else
        {
            // If it's a client, call the server to activate the wall
            ActivateFloatingWallServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ActivateFloatingWallServerRpc()
    {
        // Ensure the FloatingWall is activated across all clients
        SetFloatingWallActiveClientRpc(true);
    }

    [ClientRpc]
    private void SetFloatingWallActiveClientRpc(bool active)
    {
        if (floatingWall != null)
        {
            floatingWall.SetActive(active);
        }
    }



}
