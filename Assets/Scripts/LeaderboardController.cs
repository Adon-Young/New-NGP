using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using static PlayerCollision;

public class LeaderboardController : MonoBehaviour
{
    //playerTag Display
    public TMP_Text Red;
    public TMP_Text Blue;
    public TMP_Text Green;
    public TMP_Text Purple;

    //playerTag Display
    public TMP_Text FireTag;
    public TMP_Text WaterTag;
    public TMP_Text PlantTag;
    public TMP_Text MagicTag;

    //health variable display
    public TMP_Text FireHealthText;
    public TMP_Text WaterHealthText;
    public TMP_Text PlantHealthText;
    public TMP_Text MagicHealthText;

    //mouse score display
    public TMP_Text FireMouceText;
    public TMP_Text WaterMouceText;
    public TMP_Text PlantMouceText;
    public TMP_Text MagicMouceText;

    //statue score display
    public TMP_Text FireStatueText;
    public TMP_Text WaterStatueText;
    public TMP_Text PlantStatueText;
    public TMP_Text MagicStatueText;

    //OverallScoreDisplay...
    public TMP_Text FScore;
    public TMP_Text WScore;
    public TMP_Text PScore;
    public TMP_Text MScore;


    public LevelTimer levelTimerReference;




    public void CheckPlayers()
    {
        // Find all players tagged "Player"
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        Debug.Log("Found " + players.Length + " players tagged as 'Player'.");

        foreach (GameObject player in players)
        {
            // Debug for each player
            Debug.Log("Checking player: " + player.name);

            PlayerCollision playerCollision = player.GetComponent<PlayerCollision>();
            CatHealth catHealth = player.GetComponent<CatHealth>();
            LevelTimer levelTimer = player.GetComponent<LevelTimer>();

            if (playerCollision != null && catHealth != null && levelTimer != null)
            {
                Debug.Log("Player components found: PlayerCollision, CatHealth, LevelTimer");

                // Check the playerType and assign the appropriate tag to the TMP_Text field
                switch (playerCollision.playerType)
                {
                    case PlayerType.Fire:
                        Debug.Log("Player type is Fire");

                        if (Red != null)
                        {
                            Debug.Log("Setting Red Text");
                            Red.text = "Red";
                        }

                        if (FireTag != null)
                        {
                            Debug.Log("Setting FireTag Text");
                            FireTag.text = "Fire";
                        }

                        if (FScore != null)
                        {
                            Debug.Log("Setting Fire Score");
                            FScore.text = levelTimer.onlineScoreData.Value.levelScore_score.ToString();
                        }

                        if (FireHealthText != null)
                        {
                            Debug.Log("Setting Fire Health Text");
                            FireHealthText.text = "Health: " + catHealth.currentCatHealth.ToString();
                        }
                        break;

                    case PlayerType.Water:
                        Debug.Log("Player type is Water");

                        if (Blue != null)
                        {
                            Debug.Log("Setting Blue Text");
                            Blue.text = "Blue";
                        }

                        if (WaterTag != null)
                        {
                            Debug.Log("Setting WaterTag Text");
                            WaterTag.text = "Water";
                        }

                        if (WScore != null)
                        {
                            Debug.Log("Setting Water Score");
                            WScore.text = levelTimer.onlineScoreData.Value.levelScore_score.ToString();
                        }

                        if (WaterHealthText != null)
                        {
                            Debug.Log("Setting Water Health Text");
                            WaterHealthText.text = "Health: " + catHealth.currentCatHealth.ToString();
                        }
                        break;

                    case PlayerType.Plant:
                        Debug.Log("Player type is Plant");

                        if (Green != null)
                        {
                            Debug.Log("Setting Green Text");
                            Green.text = "Green";
                        }

                        if (PlantTag != null)
                        {
                            Debug.Log("Setting PlantTag Text");
                            PlantTag.text = "Plant";
                        }

                        if (PScore != null)
                        {
                            Debug.Log("Setting Plant Score");
                            PScore.text = levelTimer.onlineScoreData.Value.levelScore_score.ToString();
                        }

                        if (PlantHealthText != null)
                        {
                            Debug.Log("Setting Plant Health Text");
                            PlantHealthText.text = "Health: " + catHealth.currentCatHealth.ToString();
                        }
                        break;

                    case PlayerType.Magic:
                        Debug.Log("Player type is Magic");

                        if (Purple != null)
                        {
                            Debug.Log("Setting Purple Text");
                            Purple.text = "Purple";
                        }

                        if (MagicTag != null)
                        {
                            Debug.Log("Setting MagicTag Text");
                            MagicTag.text = "Magic";
                        }

                        if (MScore != null)
                        {
                            Debug.Log("Setting Magic Score");
                            MScore.text = levelTimer.onlineScoreData.Value.levelScore_score.ToString();
                        }

                        if (MagicHealthText != null)
                        {
                            Debug.Log("Setting Magic Health Text");
                            MagicHealthText.text = "Health: " + catHealth.currentCatHealth.ToString();
                        }
                        break;

                    default:
                        Debug.Log("Unknown PlayerType.");
                        break;
                }
            }
            else
            {
                Debug.LogWarning("Player components are missing on: " + player.name);
            }
        }
    }
}