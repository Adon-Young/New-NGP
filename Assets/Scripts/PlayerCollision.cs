using UnityEngine;
using Unity.Netcode;
using TMPro;


public class PlayerCollision : NetworkBehaviour
{
    public enum PlayerType { Fire, Water, Magic, Plant }
    public PlayerType playerType;


    // Network variables to track individual player scores and total scores
    //public NetworkVariable<int> networkMouseOfferings = new NetworkVariable<int>(0);  // Player's mouse score
    //public NetworkVariable<int> networkStatueScore = new NetworkVariable<int>(0);  // Player's statue score
    //public static NetworkVariable<int> totalMouseOfferings = new NetworkVariable<int>(0);  // Total mouse score for all players
    //public static NetworkVariable<int> totalStatueScore = new NetworkVariable<int>(0);  // Total statue score for all players
    LevelTimer gamesLevelTimerReference;
    NewPlayerController playerController;
    private TMP_Text mouseOfferingsText;  // Reference to the Text UI for displaying the mouse offerings score
    private TMP_Text statueScoreText;  // Reference to the Text UI for displaying the statue score
    private GameObject MouseOnCatObject;  // The GameObject that is the mouse sprite (child of the player)
    private ScoreController scoreController;
    // Network variable to sync MouseOnCat state
    public NetworkVariable<bool> mouseOnCatVisible = new NetworkVariable<bool>(false);

    public void Start()
    {
        scoreController = FindObjectOfType<ScoreController>();
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
            mouseOfferingsText.text = "Mouse Offerings: " + ScoreController.totalMouseOfferings.Value.ToString();
        }

        if (statueScoreText != null)
        {
            // Display the total statue score for all players
            statueScoreText.text = "Statue Score: " + ScoreController.totalStatueScore.Value.ToString();
        }
    }

  

    public void SetPlayerType(string tag)
    {
        switch (tag)
        {
            case "Water":
                playerType = PlayerType.Water;
        
                break;
            case "Fire":
                playerType = PlayerType.Fire;
          
                break;
            case "Plant":
                playerType = PlayerType.Plant;
          
                break;
            case "Magic":
                playerType = PlayerType.Magic;
        
                break;
            default:
                break;
        }
    }
  public PlayerType GetCurrentPlayerType()
    {
        return playerType;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Mouse"))
        {
            // Get the Mouse component from the collided object
            MouseOffering mouse = other.GetComponent<MouseOffering>();

            if (mouse != null && mouse.mouseType.ToString() == playerType.ToString())
            {
                if (NetworkManager.Singleton.LocalClientId == OwnerClientId)
                {
                    // Update the mouse offerings score and notify all clients
                    UpdateMouseOfferingsScoreServerRpc(1, OwnerClientId);

                    // Destroy the mouse object after scoring on the server
                    DestroyMouseServerRpc(other.gameObject.GetComponent<NetworkObject>());
                    PlayeMouseCaughtAudioServerRpc();//audio for catching mice

                    // Optionally, set the mouse sprite visible
                    SetMouseOnCatVisibleServerRpc(true);
                }
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

    [ServerRpc(RequireOwnership = false)]
    public void PlayeMouseCaughtAudioServerRpc()//sending info to the server
    {
        TellClientsToPlayTheMouseAudioClientRpc();//distributing the audio to all clients at once telling them to play the audio in synch
    }


    [ServerRpc(RequireOwnership = false)]
    public void DestroyMouseServerRpc(NetworkObjectReference mouseReference)
    {
        if (mouseReference.TryGet(out var mouseObject))
        {
            mouseObject.Despawn(true); // Despawn from the network
            Destroy(mouseObject.gameObject); // Destroy locally
        }
    }

    // ServerRpc to update the mouse offerings score on the server
    [ServerRpc(RequireOwnership = false)]
    public void UpdateMouseOfferingsScoreServerRpc(int addValue, ulong clientId)
    {
        scoreController.networkMouseOfferings.Value += addValue;  // Add value to the player's score
        ScoreController.totalMouseOfferings.Value += addValue;  // Update the total mouse offerings score

        NotifyMouseOfferingsScoreClientRpc(scoreController.networkMouseOfferings.Value);  // Notify all clients to update their display
    }

    // ServerRpc to update the statue score on the server
    [ServerRpc(RequireOwnership = false)]
    public void UpdateStatueScoreServerRpc(int addValue, ulong clientId)
    {
        scoreController.networkStatueScore.Value += addValue;  // Add value to the statue score
        ScoreController.totalStatueScore.Value += addValue;  // Update the total statue score

        NotifyStatueScoreClientRpc(scoreController.networkStatueScore.Value);  // Notify all clients to update their display
    }

    // ServerRpc to show/hide the mouse sprite (statue functionality can be added here)
    [ServerRpc]
    public void SetMouseOnCatVisibleServerRpc(bool isVisible)
    {
        // Update the network variable's value on the server
        mouseOnCatVisible.Value = isVisible;
    }


    [ClientRpc]
    public void TellClientsToPlayTheMouseAudioClientRpc()
    {
        AudioSource mouseAudio = GameObject.Find("MouseCaughtAudio").GetComponent<AudioSource>();
        if (mouseAudio != null)
        {
            mouseAudio.Play();
        }
    }//can use the same method for damage when a player gets hurt?--need to get more audio


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
        if (scoreController.networkStatueScore.Value == 4 && scoreController.networkMouseOfferings.Value == 4)
        {
            // Stop the game countdown timer
            if (gamesLevelTimerReference != null)
            {
                gamesLevelTimerReference.StopTimerServerRpc(); // Call to stop the timer on the server
            }
        }
    }

}
