
using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class LevelTimer : NetworkBehaviour
{
    public TMP_Text scoreText;
    public TMP_Text countdownText;
    public TMP_Text levelTimer;

    public int timeLimit = 300;
    private int score;
    private bool timerRunning = false;
    private bool levelComplete = true;  // Controls countdown and level end
    private float timeTaken = 0f;

    // Network variables for countdown, timer, score, and bools
    private NetworkVariable<MyScoreMechanics> onlineScoreData = new NetworkVariable<MyScoreMechanics>(new MyScoreMechanics { }, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // Network variable for countdown state
    private NetworkVariable<int> countdownValue = new NetworkVariable<int>(3, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server); // Start countdown from 3

    [System.Serializable]
    public struct MyScoreMechanics : INetworkSerializable
    {
        public int levelScore_score; // Score for the team
        public bool endOfLevel_levelComplete; // Is level complete?
        public bool endOfCounttDownTimer_timerRunning; // Is timer on or off

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref levelScore_score);
            serializer.SerializeValue(ref endOfLevel_levelComplete);
            serializer.SerializeValue(ref endOfCounttDownTimer_timerRunning);
        }
    }

    private void Start()
    {
        score = 0;
        UpdateScoreText();
    }

    private void Update()
    {
        // Toggle countdown/level start and level end
        if (Input.GetKeyDown(KeyCode.T) && IsServer) // Only allow the server to toggle the level state
        {
            ToggleLevelState();
        }

        // If the timer is running and level is not complete, update the timer
        if (timerRunning && !levelComplete)
        {
            timeTaken += Time.deltaTime;
            UpdateTimerTextClientRpc(timeTaken); // Send current time to clients

            if (timeTaken >= timeLimit)
            {
                StopTimerServerRpc(); // Call server function to stop timer
            }
        }

        // If countdown is running, update countdown
        if (countdownValue.Value > 0)
        {
            countdownText.text = countdownValue.Value.ToString();
        }
    }

    private IEnumerator CountdownCoroutine()
    {
        // Run the countdown
        for (int i = 3; i > 0; i--)
        {
            countdownValue.Value = i; // Update network variable
            yield return new WaitForSeconds(1f);
        }

        countdownValue.Value = 0; // Clear countdown
        UpdateTimerTextClientRpc(timeTaken); // Notify clients that the countdown is over

        // Wait a moment before starting the timer for clarity
        yield return new WaitForSeconds(1f);

        // Start the timer after the countdown is complete
        timerRunning = true;  // Start the level timer
        UpdateOnlineDataUsingLocalValues(); // Update online state to indicate timer has started
    }

    // Calculate score based on time remaining
    public void CalculateScore()
    {
        score = (int)((timeLimit - timeTaken) * 10);
        UpdateScoreText();
    }

    private void UpdateScoreText()
    {
        scoreText.text = "Score: " + score;
    }

    private void UpdateLevelTimerText()
    {
        int minutes = Mathf.FloorToInt(timeTaken / 60F);
        int seconds = Mathf.FloorToInt(timeTaken % 60F);
        levelTimer.text = $"Time: {minutes:00}:{seconds:00}";
    }

    private void ResetLevel()
    {
        Debug.Log("Time limit reached! Level reset.");
        timerRunning = false;
        levelComplete = true;  // Mark the level as complete
    }

    private void ToggleLevelState()
    {
        levelComplete = !levelComplete;

        if (levelComplete)
        {
            StopTimerServerRpc(); // Notify server to stop the timer
            Debug.Log("Level complete! Final Score: " + score);
        }
        else
        {
            Debug.Log("Starting level...");
            StartTimerServerRpc(); // Notify server to start the timer
        }
    }

    // Separate function to reset the score only when you want to
    public void ResetScore()
    {
        score = 0;
        UpdateScoreText();
        Debug.Log("Score reset.");
        UpdateOnlineDataUsingLocalValues(); // Update struct data with the new local values
    }






    //----------------------------------------------------------------------------------------------------------
    // Network functions...

    private void UpdateOnlineDataUsingLocalValues() // Fixed typo
    {
        MyScoreMechanics newScoreData = new MyScoreMechanics
        {
            levelScore_score = score,
            endOfLevel_levelComplete = levelComplete,
            endOfCounttDownTimer_timerRunning = timerRunning
        };

        onlineScoreData.Value = newScoreData; // Update the instance of the struct
    }
    //----------------------------------------------------------------------------------------------------------
    [ServerRpc]
    public void StartTimerServerRpc()
    {
        timeTaken = 0f; // Reset timer
        timerRunning = false; // Ensure timer is off initially
        StartCoroutine(CountdownCoroutine()); // Start your countdown coroutine
    }

    [ServerRpc]
    public void StopTimerServerRpc()
    {
        timerRunning = false; // Stop the timer on the server
        CalculateScore(); // Calculate the score on the server
        UpdateOnlineDataUsingLocalValues(); // Update the online score data
        UpdateTimerTextClientRpc(timeTaken); // Send final time to clients
        UpdateScoreClientRpc(score); // Send the score to clients
    }

    [ServerRpc]
    public void UpdateScoreOnlineServerRpc(MyScoreMechanics newScoreData)
    {
        onlineScoreData.Value = newScoreData; // Updates all values in the struct at once
    }
    //----------------------------------------------------------------------------------------------------------
    [ClientRpc]
    public void StopTimerClientRpc()
    {
        // Clients should not call this method directly, it is invoked by the server
        if (IsServer) // Ensure only the server processes stopping
        {
            timerRunning = false; // Stop the local timer for the server
            CalculateScore(); // Calculate the score on the server
            UpdateOnlineDataUsingLocalValues(); // Update the online score data
        }
    }

    [ClientRpc]
    private void UpdateTimerTextClientRpc(float time)
    {
        timeTaken = time; // Sync time with clients
        UpdateLevelTimerText(); // Update the displayed time
    }

    [ClientRpc]
    private void UpdateScoreClientRpc(int finalScore)
    {
        score = finalScore; // Update the local score for the client
        UpdateScoreText(); // Refresh the displayed score on the client side
    }
}
