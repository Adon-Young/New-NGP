using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BeginGame : NetworkBehaviour
{   
    public NetworkVariable<int> characterSelected = new NetworkVariable<int>(0);  // Score for Mouse Offerings
    private int numberOfCharactersSelected = 0;  // Local score for Mouse Offerings
    LevelTimer gamesLevelTimerReference;
    public GameObject charSelectionScreen;

    // Start is called before the first frame update
    private void Awake()
    {
        if (gamesLevelTimerReference == null)
        {
            gamesLevelTimerReference = FindObjectOfType<LevelTimer>();
        }
    }





    public void Update()
    {
        OnAllPlayersSelected();
    }




    [ServerRpc(RequireOwnership = false)]
    public void UpdateCharacterSelectionServerRpc(int addValue, ulong clientId)
    {
        if (characterSelected.Value < 4) // Ensure only up to 4 characters can be selected
        {
            characterSelected.Value += addValue; // Increment the network variable
            Debug.Log("Player " + clientId + " selected a character. Total selected: " + characterSelected.Value);

            UpdateCharacterSelectionClientRpc(characterSelected.Value); // Notify all clients
        }
        else
        {
            Debug.LogWarning("All characters have been selected! No more selections allowed.");
        }
    }

    [ClientRpc]
    private void UpdateCharacterSelectionClientRpc(int newScore)
    {
        numberOfCharactersSelected = newScore; // Update the local value
        Debug.Log("Updated local count to: " + numberOfCharactersSelected);
    }

    public void OnCharacterButtonClicked()
    {
        if (IsClient && !HasAlreadySelected()) // Ensure client is valid and hasn't selected yet
        {
            Debug.Log("Character has been selected");
            UpdateCharacterSelectionServerRpc(1, NetworkManager.Singleton.LocalClientId); // Notify the server
        }
      
    }
    //players shouldnt be able to select more than oonce as ive added in the blockers for the buttoons but just in case...
    private bool HasAlreadySelected()
    {
        return numberOfCharactersSelected >= 4; // Prevent extra selections after limit
    }


    private void OnAllPlayersSelected()
    {
        if (characterSelected.Value == 4)
        {
            Debug.Log("All players have selected their characters! Transitioning...");

            // Transition UI
            charSelectionScreen.SetActive(false);


            // Start game countdown timer
            if (gamesLevelTimerReference != null)
            {
                
                gamesLevelTimerReference.StartTimerServerRpc(); // Trigger the countdown and timer start
            }
            else
            {
                Debug.LogError("LevelTimer reference is missing!");
            }
        }
    }
}
