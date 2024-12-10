using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class CatHealth : NetworkBehaviour
{
    public NetworkVariable<int> currentCatHealth = new NetworkVariable<int>(3);
    public NetworkVariable<bool> catHasDied = new NetworkVariable<bool>(false);  
    private int maxCatHealth = 3;
    private int minCatHealth = 0;
    public Text HealthTextOnCanvas;//making it public do i don't need to find it dynamically
    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        CheckForCanvasTMP();
    }


    private void CheckForCanvasTMP()
    {
        if (HealthTextOnCanvas != null)
        {
            //displayCatHealth
            HealthTextOnCanvas.text = currentCatHealth.Value.ToString();//displaying the int as a string value for the TMP
        }
    }


    [ServerRpc(RequireOwnership = false)]
    public void UpdateCatHealthServerRpc(int addValue, ulong clientId)
    {
        currentCatHealth.Value += addValue;  
        Debug.Log("The Statue score of player " + clientId + " is " + currentCatHealth.Value);
        UpdateCatHelathClientRpc(currentCatHealth.Value);
    }

    [ClientRpc]
    private void UpdateCatHelathClientRpc(int newScore)
    {
        
        if (HealthTextOnCanvas != null)
        {
            HealthTextOnCanvas.text = "" + newScore.ToString();
        }
    }


}
