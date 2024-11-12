using UnityEngine;
using Unity.Netcode;
using TMPro;

public class PlayerCollision : NetworkBehaviour
{
    public NetworkVariable<int> networkMouseOfferings = new NetworkVariable<int>(0);  // Score for Mouse Offerings
    public NetworkVariable<int> networkStatueScore = new NetworkVariable<int>(0);  // Score for Statue

    private int clientMouseOfferings = 0;  // Local score for Mouse Offerings
    private int clientStatueScore = 0;  // Local score for Statue

    private TMP_Text mouseOfferingsText;  // Reference to the Text UI for displaying the mouse offerings score
    private TMP_Text statueScoreText;  // Reference to the Text UI for displaying the statue score

    private GameObject MouseOnCatObject;  // The GameObject that is the mouse sprite (child of the player)
    private NewPlayerController playerController;  // Reference to the player controller

    // Network variable to sync MouseOnCat state
    public NetworkVariable<bool> mouseOnCatVisible = new NetworkVariable<bool>(false);

    public void Start()
    {
        // Dynamically find the Mouse Offerings and Statue Score Text objects in the Canvas
        mouseOfferingsText = GameObject.Find("MouseOfferings").GetComponent<TMP_Text>();
        statueScoreText = GameObject.Find("StatueScore").GetComponent<TMP_Text>();  // Assuming statue score is displayed as well

        // Set up the local reference to MouseOnCatObject and ensure it starts inactive
        MouseOnCatObject = this.gameObject.transform.GetChild(1).gameObject;  // Assuming it's the second child
        MouseOnCatObject.SetActive(false);  // Initially hide the mouse sprite

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

        playerController = GetComponent<NewPlayerController>();
        // Update the score displays on the client if the Text objects are found
        if (mouseOfferingsText != null)
        {
            mouseOfferingsText.text = "Mouse Offerings: " + clientMouseOfferings.ToString();
        }

        if (statueScoreText != null)
        {
            statueScoreText.text = "Statue Score: " + clientStatueScore.ToString();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Mouse"))
        {
            if (NetworkManager.Singleton.LocalClientId == OwnerClientId)
            {
                // Update the mouse offerings score and notify all clients
                UpdateMouseOfferingsScoreServerRpc(1, OwnerClientId);

                // Destroy the mouse object after scoring
                Destroy(other.gameObject);

                // Set the mouse sprite child object to be visible
                SetMouseOnCatVisibleServerRpc(true);
            }
        }

        if (other.CompareTag("Statue"))
        {
            // Update the statue score when a player enters the statue area
            if (NetworkManager.Singleton.LocalClientId == OwnerClientId)
            {
                UpdateStatueScoreServerRpc(1, OwnerClientId);
            }
        }

        // Water collision logic
        if (other.CompareTag("Water") && playerController.isWaterWorld == true)
        {
            playerController.EnterWater();
        }
        else if (other.CompareTag("Water") && playerController.isWaterWorld != true)
        {
            playerController.EnterWater();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Statue"))
        {
            // Decrease the statue score when the player exits the statue area
            if (NetworkManager.Singleton.LocalClientId == OwnerClientId)
            {
                UpdateStatueScoreServerRpc(-1, OwnerClientId);
            }
        }

        if (other.CompareTag("Water") && playerController.isWaterWorld == true)
        {
            playerController.ExitWater();
        }
    }

    // ServerRpc to update the mouse offerings score on the server
    [ServerRpc(RequireOwnership = false)]
    public void UpdateMouseOfferingsScoreServerRpc(int addValue, ulong clientId)
    {
        networkMouseOfferings.Value += addValue;  // Add value to the score on the server
        Debug.Log("The Mouse Offerings score of player " + clientId + " is " + networkMouseOfferings.Value);
        NotifyMouseOfferingsScoreClientRpc(networkMouseOfferings.Value);  // Notify all clients to update their display
    }

    // ServerRpc to update the statue score on the server
    [ServerRpc(RequireOwnership = false)]
    public void UpdateStatueScoreServerRpc(int addValue, ulong clientId)
    {
        networkStatueScore.Value += addValue;  // Add value to the statue score on the server
        Debug.Log("The Statue score of player " + clientId + " is " + networkStatueScore.Value);
        NotifyStatueScoreClientRpc(networkStatueScore.Value);  // Notify all clients to update their display
    }

    // ServerRpc to show/hide the mouse sprite (statue functionality can be added here)
    [ServerRpc]
    public void SetMouseOnCatVisibleServerRpc(bool isVisible)
    {
        // Update the network variable's value on the server
        mouseOnCatVisible.Value = isVisible;
    }

    // ClientRpc to notify all clients about the updated mouse offerings score
    [ClientRpc]
    private void NotifyMouseOfferingsScoreClientRpc(int newScore)
    {
        clientMouseOfferings = newScore;

        // Update the UI to reflect the new score
        if (mouseOfferingsText != null)
        {
            mouseOfferingsText.text = "Mouse Offerings: " + clientMouseOfferings.ToString();
        }
    }

    // ClientRpc to notify all clients about the updated statue score
    [ClientRpc]
    private void NotifyStatueScoreClientRpc(int newScore)
    {
        clientStatueScore = newScore;

        // Update the UI to reflect the new statue score
        if (statueScoreText != null)
        {
            statueScoreText.text = "Statue Score: " + clientStatueScore.ToString();
        }
    }
}
