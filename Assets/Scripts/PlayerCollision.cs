using UnityEngine;
using Unity.Netcode;
using TMPro;

public class PlayerCollision : NetworkBehaviour
{
    // Network variable for player's score with default initialization to 0
    public NetworkVariable<int> networkPlayerScore = new NetworkVariable<int>(0);
    public int clientScore = 0; // Local score variable

    // UI text components for displaying the score in different worlds
    public TMP_Text playerScoreDisplayWater;
    public TMP_Text playerScoreDisplayFire;
    public TMP_Text playerScoreDisplayPlant;
    public TMP_Text playerScoreDisplayMagic;
    private GameObject MouseOnCatObject;

    NewPlayerController playerController; // Reference to player controller



    public void Start()
    {
        //finding local mouse...

      
        MouseOnCatObject.SetActive(false);

    }
    public void Update()
    {
        MouseOnCatObject = this.gameObject.transform.GetChild(1).gameObject;

        // Get the player's world type
        playerController = GetComponent<NewPlayerController>();

        // Based on the world the player is in, find the corresponding score display UI
        if (playerController.isWaterWorld)
        {
            GameObject scoreTextObject = GameObject.Find("WaterWorldScoreTMP");
            if (scoreTextObject != null)
            {
                playerScoreDisplayWater = scoreTextObject.GetComponent<TMP_Text>();
            }
        }
        else if (playerController.isFireWorld)
        {
            GameObject scoreTextObject = GameObject.Find("FireWorldScoreTMP");
            if (scoreTextObject != null)
            {
                playerScoreDisplayFire = scoreTextObject.GetComponent<TMP_Text>();
            }
        }
        else if (playerController.isPlantWorld)
        {
            GameObject scoreTextObject = GameObject.Find("PlantWorldScoreTMP");
            if (scoreTextObject != null)
            {
                playerScoreDisplayPlant = scoreTextObject.GetComponent<TMP_Text>();
            }
        }
        else if (playerController.isMagicWorld)
        {
            GameObject scoreTextObject = GameObject.Find("MagicWorldScoreTMP");
            if (scoreTextObject != null)
            {
                playerScoreDisplayMagic = scoreTextObject.GetComponent<TMP_Text>();
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        //change state so that if it is statue and the player has a mouse then wait to trigger end of level
        if (other.CompareTag("Statue"))
        {
            // Check if the local client is the owner of the player object
            if (NetworkManager.Singleton.LocalClientId == OwnerClientId)
            {
                // Call the server RPC to update the score
                UpdateScoreServerRpc(1, OwnerClientId);
            }
        }
        //on trigger for the mouse for offering colleciton...

        if(other.CompareTag("Mouse"))
        {
            if (NetworkManager.Singleton.LocalClientId == OwnerClientId)
            {
                
                // Call the server RPC to update the score
                UpdateScoreServerRpc(1, OwnerClientId);
                Destroy(other.gameObject);
                MouseOnCatObject.SetActive(true);
            }
        }



    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateScoreServerRpc(int addValue, ulong clientId)
    {
        // Update the score on the server side for the correct player
        networkPlayerScore.Value += addValue;

        // Log the score in the console for debugging
        Debug.Log("The score of player " + clientId + " is " + networkPlayerScore.Value);

        // Notify all clients about the score update
        NotifyScoreClientRpc(networkPlayerScore.Value);
    }

    [ClientRpc]
    private void NotifyScoreClientRpc(int newScore)
    {
        // Log to make sure the client RPC is being called correctly
        Debug.Log("NotifyScoreClientRpc called with new score: " + newScore);

        // Update the local score
        clientScore = newScore;

        // Update the score display UI based on the player's current world
        if (playerController.isWaterWorld && playerScoreDisplayWater != null)
        {
            Debug.Log("Updating WaterWorld score UI.");
            playerScoreDisplayWater.text = "Water Cat Score: " + clientScore.ToString();
        }
        else if (playerController.isFireWorld && playerScoreDisplayFire != null)
        {
            Debug.Log("Updating FireWorld score UI.");
            playerScoreDisplayFire.text = "Fire Cat Score: " + clientScore.ToString();
        }
        else if (playerController.isPlantWorld && playerScoreDisplayPlant != null)
        {
            Debug.Log("Updating PlantWorld score UI.");
            playerScoreDisplayPlant.text = "Plant Cat Score: " + clientScore.ToString();
        }
        else if (playerController.isMagicWorld && playerScoreDisplayMagic != null)
        {
            Debug.Log("Updating MagicWorld score UI.");
            playerScoreDisplayMagic.text = "Magic Cat Score: " + clientScore.ToString();
        }
        else
        {
            Debug.LogError("No score display UI found for the current world!");
        }
    }

}
