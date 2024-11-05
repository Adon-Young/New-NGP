using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class LevelController : NetworkBehaviour
{
    public GameObject WaterSceneLLv1;
    public GameObject FireSceneLv1;
    public GameObject PlantSceneLv1;
    public GameObject MagicSceneLv1;

 

    private void Awake()
    {
        // Default all scenes off
        WaterSceneLLv1.SetActive(false);
        FireSceneLv1.SetActive(false);
        PlantSceneLv1.SetActive(false);
        MagicSceneLv1.SetActive(false);
    }

    // Accept the player's NewPlayerController as a parameter
    public void WorldChecker(NewPlayerController playerController)
    {
        if (playerController != null)
        {
            if (playerController.isWaterWorld)
            {
                WaterSceneLLv1.SetActive(true);
                Debug.Log("Water Scene is now visible.");
            }
            if (playerController.isFireWorld)
            {
                FireSceneLv1.SetActive(true);
                Debug.Log("Fire Scene is now visible.");
            }
            if (playerController.isPlantWorld)
            {
                PlantSceneLv1.SetActive(true);
                Debug.Log("Plant Scene is now visible.");
            }
            if (playerController.isMagicWorld)
            {
                MagicSceneLv1.SetActive(true);
                Debug.Log("Magic Scene is now visible.");
            }
        }
        else
        {
            Debug.LogError("PlayerController reference is null!");
        }
    }



}
