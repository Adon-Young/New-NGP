
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
        if (IsServer && timerRunning && !levelComplete)
        {
            timeTaken += Time.deltaTime; // Update time only on server
            UpdateTimerTextClientRpc(timeTaken); // Send updated time to all clients

            if (timeTaken >= timeLimit)
            {
                StopTimerServerRpc(); // Stop the timer when time is up
            }
        }

        // Countdown display on the client
        if (countdownValue.Value > 0)
        {
            countdownText.text = countdownValue.Value.ToString();
        }
    }

    private IEnumerator CountdownCoroutine()
    {
        for (int i = 3; i > 0; i--)
        {
            countdownValue.Value = i; // Sync countdown value across network
            yield return new WaitForSeconds(1f);
        }

        countdownValue.Value = 0; // Countdown complete
        UpdateTimerTextClientRpc(timeTaken); // Notify clients that the countdown is done
        yield return new WaitForSeconds(1f); // Wait briefly before starting the timer

        // Start the game timer on the server
        timerRunning = true;  // Start the level timer
        UpdateOnlineDataUsingLocalValues(); // Sync state to all clients
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

    private void UpdateOnlineDataUsingLocalValues()
    {
        MyScoreMechanics newScoreData = new MyScoreMechanics
        {
            levelScore_score = score,
            endOfLevel_levelComplete = levelComplete,
            endOfCounttDownTimer_timerRunning = timerRunning
        };

        onlineScoreData.Value = newScoreData; // Update the network variable with server-side data
    }
    //----------------------------------------------------------------------------------------------------------
    [ServerRpc]
    public void StartTimerServerRpc()
    {
        if (timerRunning) // Check if timer is already running
        {
            Debug.LogWarning("Timer already running. Cannot start again.");
            return;
        }

        // Initialize values server-side
        timeTaken = 0f; // Reset time
        levelComplete = false; // Ensure level is not complete
        timerRunning = false; // Ensure timer is off initially

        // Start countdown and begin the game timer once it finishes
        StartCoroutine(CountdownCoroutine());
    }

    [ServerRpc]
    public void StopTimerServerRpc()
    {
        timerRunning = false; // Stop the timer on the server
        CalculateScore(); // Calculate the score on the server

        // Update all clients with the final state
        UpdateOnlineDataUsingLocalValues();
        UpdateTimerTextClientRpc(timeTaken);
        UpdateScoreClientRpc(score);
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
        timeTaken = time; // Sync the time value with clients
        UpdateLevelTimerText(); // Update the timer text on the client UI
    }

    [ClientRpc]
    private void UpdateScoreClientRpc(int finalScore)
    {
        score = finalScore; // Update the local score for the client
        UpdateScoreText(); // Refresh the displayed score on the client side
    }
}
