using UnityEngine;
using Unity.Netcode;
using TMPro;

public class PlayerCollision : NetworkBehaviour
{
    public NetworkVariable<int> networkPlayerScore = new NetworkVariable<int>(0);
    public int clientScore = 0; // Local score variable

    // Network variable to sync MouseOnCat state
    public NetworkVariable<bool> mouseOnCatVisible = new NetworkVariable<bool>(false);

    // UI text components for displaying the score in different worlds
    public TMP_Text playerScoreDisplayWater;
    public TMP_Text playerScoreDisplayFire;
    public TMP_Text playerScoreDisplayPlant;
    public TMP_Text playerScoreDisplayMagic;

    private GameObject MouseOnCatObject;
    private NewPlayerController playerController; // Reference to player controller

    public void Start()
    {
        // Set up the local reference to MouseOnCatObject and ensure it starts inactive
        MouseOnCatObject = this.gameObject.transform.GetChild(1).gameObject;
        MouseOnCatObject.SetActive(false);

        // Subscribe to the network variable's change event
        mouseOnCatVisible.OnValueChanged += OnMouseOnCatVisibilityChanged;
    }

    private void OnDestroy()
    {
        // Unsubscribe from the event to avoid memory leaks
        mouseOnCatVisible.OnValueChanged -= OnMouseOnCatVisibilityChanged;
    }

    private void OnMouseOnCatVisibilityChanged(bool previousValue, bool newValue)
    {
        // Update the visibility of MouseOnCatObject based on the network variable's value
        MouseOnCatObject.SetActive(newValue);
    }

    public void Update()
    {
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
        if (other.CompareTag("Statue"))
        {
            if (NetworkManager.Singleton.LocalClientId == OwnerClientId)
            {
                UpdateScoreServerRpc(1, OwnerClientId);
            }
        }

        if (other.CompareTag("Mouse"))
        {
            if (NetworkManager.Singleton.LocalClientId == OwnerClientId)
            {
                UpdateScoreServerRpc(1, OwnerClientId);

                // Destroy the mouse object and set the network variable to make MouseOnCat visible
                Destroy(other.gameObject);
                SetMouseOnCatVisibleServerRpc(true);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateScoreServerRpc(int addValue, ulong clientId)
    {
        networkPlayerScore.Value += addValue;
        Debug.Log("The score of player " + clientId + " is " + networkPlayerScore.Value);
        NotifyScoreClientRpc(networkPlayerScore.Value);
    }

    [ServerRpc]
    public void SetMouseOnCatVisibleServerRpc(bool isVisible)
    {
        // Update the network variable's value on the server
        mouseOnCatVisible.Value = isVisible;
    }

    [ClientRpc]
    private void NotifyScoreClientRpc(int newScore)
    {
        Debug.Log("NotifyScoreClientRpc called with new score: " + newScore);
        clientScore = newScore;

        if (playerController.isWaterWorld && playerScoreDisplayWater != null)
        {
            playerScoreDisplayWater.text = "Water Cat Score: " + clientScore.ToString();
        }
        else if (playerController.isFireWorld && playerScoreDisplayFire != null)
        {
            playerScoreDisplayFire.text = "Fire Cat Score: " + clientScore.ToString();
        }
        else if (playerController.isPlantWorld && playerScoreDisplayPlant != null)
        {
            playerScoreDisplayPlant.text = "Plant Cat Score: " + clientScore.ToString();
        }
        else if (playerController.isMagicWorld && playerScoreDisplayMagic != null)
        {
            playerScoreDisplayMagic.text = "Magic Cat Score: " + clientScore.ToString();
        }
        else
        {
            Debug.LogError("No score display UI found for the current world!");
        }
    }
}
