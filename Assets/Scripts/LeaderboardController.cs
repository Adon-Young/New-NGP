using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class LeaderboardController : NetworkBehaviour
{
    private NewPlayerController newPlayerController;
    private PlayerCollision playerCollision;
    private CatHealth catHealth;
    private EndOfGame endOfGameReference;
    //display colour
    public TMP_Text Red;
    public TMP_Text Blue;
    public TMP_Text Green;
    public TMP_Text Purple;

    // Player Tag Display
    public TMP_Text FireTag;
    public TMP_Text WaterTag;
    public TMP_Text PlantTag;
    public TMP_Text MagicTag;

    // Health Variable Display
    public TMP_Text FireHealthText;
    public TMP_Text WaterHealthText;
    public TMP_Text PlantHealthText;
    public TMP_Text MagicHealthText;

    // Mouse Score Display
    public TMP_Text FireMouseText;
    public TMP_Text WaterMouseText;
    public TMP_Text PlantMouseText;
    public TMP_Text MagicMouseText;

    // Statue Score Display
    public TMP_Text FireStatueText;
    public TMP_Text WaterStatueText;
    public TMP_Text PlantStatueText;
    public TMP_Text MagicStatueText;

    // Overall Score Display
    public TMP_Text FScore;
    public TMP_Text WScore;
    public TMP_Text PScore;
    public TMP_Text MScore;



    public void Search()
    {

        Red = GameObject.Find("RedText").GetComponent<TMP_Text>();
        Blue = GameObject.Find("BlueText").GetComponent<TMP_Text>();
        Green = GameObject.Find("GreenText").GetComponent<TMP_Text>();
        Purple = GameObject.Find("PurpleText").GetComponent<TMP_Text>();

        FireTag = GameObject.Find("FireTagText").GetComponent<TMP_Text>();
        WaterTag = GameObject.Find("WaterTagText").GetComponent<TMP_Text>();
        PlantTag = GameObject.Find("PlantTagText").GetComponent<TMP_Text>();
        MagicTag = GameObject.Find("MagicTagText").GetComponent<TMP_Text>();

        FireHealthText = GameObject.Find("FireHealthText").GetComponent<TMP_Text>();
        WaterHealthText = GameObject.Find("WaterHealthText").GetComponent<TMP_Text>();
        PlantHealthText = GameObject.Find("PlantHealthText").GetComponent<TMP_Text>();
        MagicHealthText = GameObject.Find("MagicHealthText").GetComponent<TMP_Text>();

        FireMouseText = GameObject.Find("FireMouseText").GetComponent<TMP_Text>();
        WaterMouseText = GameObject.Find("WaterMouseText").GetComponent<TMP_Text>();
        PlantMouseText = GameObject.Find("PlantMouseText").GetComponent<TMP_Text>();
        MagicMouseText = GameObject.Find("MagicMouseText").GetComponent<TMP_Text>();

        FireStatueText = GameObject.Find("FireStatueText").GetComponent<TMP_Text>();
        WaterStatueText = GameObject.Find("WaterStatueText").GetComponent<TMP_Text>();
        PlantStatueText = GameObject.Find("PlantStatueText").GetComponent<TMP_Text>();
        MagicStatueText = GameObject.Find("MagicStatueText").GetComponent<TMP_Text>();

        FScore = GameObject.Find("FScoreText").GetComponent<TMP_Text>();
        WScore = GameObject.Find("WScoreText").GetComponent<TMP_Text>();
        PScore = GameObject.Find("PScoreText").GetComponent<TMP_Text>();
        MScore = GameObject.Find("MScoreText").GetComponent<TMP_Text>();
    }

    // Function to access the required scripts from the current GameObject
    public void InitializePlayerComponents()
    {
        newPlayerController = this.gameObject.GetComponent<NewPlayerController>();
        playerCollision = this.gameObject.GetComponent<PlayerCollision>();
        catHealth = this.gameObject.GetComponent<CatHealth>();
        endOfGameReference = GameObject.Find("LevelController").GetComponent<EndOfGame>();


        if (newPlayerController == null)
            Debug.LogError("NewPlayerController script not found on " + this.gameObject.name);
        if (playerCollision == null)
            Debug.LogError("PlayerCollision script not found on " + this.gameObject.name);
        if (catHealth == null)
            Debug.LogError("CatHealth script not found on " + this.gameObject.name);
    }

    // Function to check the world type based on the NewPlayerController flags



    public void Update()
    {
        InitializePlayerComponents();
        Search();
        GetPlayerWorldType();
    }
    public void GetPlayerWorldType()
    {
        if (newPlayerController == null)
        {
            Debug.LogError("NewPlayerController is not initialized!");
         
        }

        if (newPlayerController.isWaterWorld)
        {
            Debug.Log("Water World detected.");
            DisplayWaterInfo();
       
        }
        else if (newPlayerController.isFireWorld)
        {
            Debug.Log("Fire World detected.");
            DisplayFireInfo();
      
        }
        else if (newPlayerController.isPlantWorld)
        {
            Debug.Log("Plant World detected.");
            DisplayPlantInfo();
     
        }
        else if (newPlayerController.isMagicWorld)
        {
            Debug.Log("Magic World detected.");
            DisplayMagicInfo();
      
        }
        else
        {
            Debug.Log("No world detected.");
      
        }
    }

    private void DisplayFireInfo()
    {
        Debug.Log("Displaying Fire World Info");
        if (Red != null) Red.text = "Red"; // Example: setting color to Red
        if (FireTag != null) FireTag.text = "Fire";

        // Replace with actual dynamic data as needed
        if (FScore != null) FScore.text = "100"; // Example score
        if (FireMouseText != null) FireMouseText.text = "3 Mice Collected";
        if (FireStatueText != null) FireStatueText.text = "1 Statue Active";
        if (FireHealthText != null) FireHealthText.text = "80 HP";
    }

    // Display information for Water World
    private void DisplayWaterInfo()
    {
        Debug.Log("Displaying Water World Info");
        if (Blue != null) Blue.text = "Blue";
        if (WaterTag != null) WaterTag.text = "Water";

        // Replace with actual dynamic data as needed
        if (WScore != null) WScore.text = "200";
        if (WaterMouseText != null) WaterMouseText.text = "5 Mice Collected";
        if (WaterStatueText != null) WaterStatueText.text = "2 Statues Active";
        if (WaterHealthText != null) WaterHealthText.text = "90 HP";
    }

    // Display information for Plant World
    private void DisplayPlantInfo()
    {
        Debug.Log("Displaying Plant World Info");
        if (Green != null) Green.text = "Green";
        if (PlantTag != null) PlantTag.text = "Plant";

        // Replace with actual dynamic data as needed
        if (PScore != null) PScore.text = "150";
        if (PlantMouseText != null) PlantMouseText.text = "4 Mice Collected";
        if (PlantStatueText != null) PlantStatueText.text = "3 Statues Active";
        if (PlantHealthText != null) PlantHealthText.text = "70 HP";
    }

    // Display information for Magic World
    private void DisplayMagicInfo()
    {
        Debug.Log("Displaying Magic World Info");
        if (Purple != null) Purple.text = "Purple";
        if (MagicTag != null) MagicTag.text = "Magic";

        // Replace with actual dynamic data as needed
        if (MScore != null) MScore.text = "300";
        if (MagicMouseText != null) MagicMouseText.text = "6 Mice Collected";
        if (MagicStatueText != null) MagicStatueText.text = "4 Statues Active";
        if (MagicHealthText != null) MagicHealthText.text = "95 HP";
    }
}
