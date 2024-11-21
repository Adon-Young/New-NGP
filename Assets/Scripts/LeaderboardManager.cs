using UnityEngine;
using TMPro;

public class LeaderboardManager : MonoBehaviour
{
    //basic leaderboard set up andtesting for UI.. will need to add in the game time along with team name and rank, rank would be determined by the fastest team time
    public GameObject leaderboardContainer; // The parent object with Vertical Layout
    public GameObject rowPrefab; // Your row prefab

    void Start()
    {
        // Example data
        string[] teamNames = { "Team Alpha", "Team Bravo", "Team Charlie" };
        float[] times = { 45.23f, 50.89f, 60.12f };

        // Populate the leaderboard
        PopulateLeaderboard(teamNames, times);
    }

    public void PopulateLeaderboard(string[] teamNames, float[] times)
    {
        // Clear existing rows
        foreach (Transform child in leaderboardContainer.transform)
        {
            Destroy(child.gameObject);
        }

        // Add new rows
        for (int i = 0; i < teamNames.Length; i++)
        {
            // Create a new row
            GameObject row = Instantiate(rowPrefab, leaderboardContainer.transform);

            // Debug log the instantiation
            Debug.Log("Instantiated row for: " + teamNames[i]);

            // Set Rank
            TMP_Text rankText = row.transform.Find("RankImage/Text").GetComponent<TMP_Text>();
            if (rankText != null)
            {
                rankText.text = (i + 1).ToString();
            }
            else
            {
                Debug.LogError("Rank TMP_Text not found in prefab!");
            }

            // Set Team Name
            TMP_Text teamNameText = row.transform.Find("TeamNameImage/Text").GetComponent<TMP_Text>();
            if (teamNameText != null)
            {
                teamNameText.text = teamNames[i];
            }
            else
            {
                Debug.LogError("Team Name TMP_Text not found in prefab!");
            }
            // Set Time
            TMP_Text timeText = row.transform.Find("TimeImage/Text").GetComponent<TMP_Text>();
            if (timeText != null)
            {
                timeText.text = times[i].ToString("F2") + "s";
                Debug.Log($"Time for row {i}: {timeText.text}");
            }
            else
            {
                Debug.LogError("Time TMP_Text not found in prefab!");
            }

        }
    }
}
