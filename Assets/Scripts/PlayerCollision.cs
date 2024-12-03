using UnityEngine;
using Unity.Netcode;
using TMPro;

public class PlayerCollision : NetworkBehaviour
{
    // Network variables to track individual player scores and total scores
    public NetworkVariable<int> networkMouseOfferings = new NetworkVariable<int>(0);  // Player's mouse score
    public NetworkVariable<int> networkStatueScore = new NetworkVariable<int>(0);  // Player's statue score
    public static NetworkVariable<int> totalMouseOfferings = new NetworkVariable<int>(0);  // Total mouse score for all players
    public static NetworkVariable<int> totalStatueScore = new NetworkVariable<int>(0);  // Total statue score for all players
    LevelTimer gamesLevelTimerReference;
    NewPlayerController playerController;
    private TMP_Text mouseOfferingsText;  // Reference to the Text UI for displaying the mouse offerings score
    private TMP_Text statueScoreText;  // Reference to the Text UI for displaying the statue score

    private GameObject MouseOnCatObject;  // The GameObject that is the mouse sprite (child of the player)

    // Network variable to sync MouseOnCat state
    public NetworkVariable<bool> mouseOnCatVisible = new NetworkVariable<bool>(false);

    public void Start()
    {

        playerController = GetComponent<NewPlayerController>();


        // Dynamically find the Mouse Offerings and Statue Score Text objects in the Canvas
        mouseOfferingsText = GameObject.Find("MouseOfferings").GetComponent<TMP_Text>();
        statueScoreText = GameObject.Find("StatueScore").GetComponent<TMP_Text>();

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

        CheckAndStopTimer();

        // Update the score displays on the client if the Text objects are found
        if (mouseOfferingsText != null)
        {
            // Display the total mouse offerings score for all players
            mouseOfferingsText.text = "Mouse Offerings: " + totalMouseOfferings.Value.ToString();
        }

        if (statueScoreText != null)
        {
            // Display the total statue score for all players
            statueScoreText.text = "Statue Score: " + totalStatueScore.Value.ToString();
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
        if (other.CompareTag("Water"))
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
  
        // Water exit logic
        if (other.CompareTag("Water"))
        {
            playerController.ExitWater();
        }
    }

    // ServerRpc to update the mouse offerings score on the server
    [ServerRpc(RequireOwnership = false)]
    public void UpdateMouseOfferingsScoreServerRpc(int addValue, ulong clientId)
    {
        networkMouseOfferings.Value += addValue;  // Add value to the player's score
        totalMouseOfferings.Value += addValue;  // Update the total mouse offerings score
        Debug.Log("The Mouse Offerings score of player " + clientId + " is " + networkMouseOfferings.Value);
        NotifyMouseOfferingsScoreClientRpc(networkMouseOfferings.Value);  // Notify all clients to update their display
    }

    // ServerRpc to update the statue score on the server
    [ServerRpc(RequireOwnership = false)]
    public void UpdateStatueScoreServerRpc(int addValue, ulong clientId)
    {
        networkStatueScore.Value += addValue;  // Add value to the statue score
        totalStatueScore.Value += addValue;  // Update the total statue score
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
        // Update the UI to reflect the new score
        if (mouseOfferingsText != null)
        {
            mouseOfferingsText.text = "Mouse Offerings: " + newScore.ToString();
        }
    }

    // ClientRpc to notify all clients about the updated statue score
    [ClientRpc]
    private void NotifyStatueScoreClientRpc(int newScore)
    {
        // Update the UI to reflect the new statue score
        if (statueScoreText != null)
        {
            statueScoreText.text = "Statue Score: " + newScore.ToString();
        }
    }


    private void CheckAndStopTimer()
    {
        // Check if both network scores have reached 4
        if (networkStatueScore.Value == 4 && networkMouseOfferings.Value == 4)
        {
            Debug.Log("Both statue and mouse offering scores are 4! Stopping the timer...");

            // Stop the game countdown timer
            if (gamesLevelTimerReference != null)
            {
                gamesLevelTimerReference.StopTimerServerRpc(); // Call to stop the timer on the server
            }
            else
            {
                Debug.LogError("LevelTimer reference is missing!");
            }
        }
    }

}
