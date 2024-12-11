using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class CatHealth : NetworkBehaviour
{
    public NetworkVariable<int> currentCatHealth = new NetworkVariable<int>(3);
    private int maxCatHealth = 3;
    private int minCatHealth = 0;
    public Text HealthTextOnCanvas;

    private GameObject fireSpawnPoint;
    private GameObject plantSpawnPoint;
    private GameObject waterSpawnPoint;
    private GameObject magicSpawnPoint;
    private bool hasTakenDamage = false; // Prevent continuous damage
    private bool isSearchingForSpawnPoint = true; // Flag to manage the delayed search

    private NewPlayerController playerController;

    void Start()
    {
        // Get reference to the player controller
        playerController = GetComponent<NewPlayerController>();
        if (playerController == null)
        {
            Debug.LogError("NewPlayerController script is missing on the GameObject!");
        }

        // Start the delayed search for the SpawnPoints
        StartCoroutine(DelayedSpawnPointSearch());
    }

    void Update()
    {
        if (!IsOwner) return; // Only the owning client handles local logic

        if (!isSearchingForSpawnPoint && (fireSpawnPoint == null || plantSpawnPoint == null || waterSpawnPoint == null || magicSpawnPoint == null))
        {
            Debug.LogWarning("One or more SpawnPoints still not found. Ensure they exist in the scene.");
        }

        CheckForDamage();
        UpdateHealthOnCanvas();
    }

    private void UpdateHealthOnCanvas()
    {
        if (HealthTextOnCanvas != null)
        {
            HealthTextOnCanvas.text = currentCatHealth.Value.ToString();
        }
    }

    private void CheckForDamage()
    {
        if (playerController != null && playerController.isInWater && !hasTakenDamage)
        {
            Debug.Log("Player is in water and has not taken damage yet.");

            if (playerController.isFireWorld || playerController.isPlantWorld || playerController.isMagicWorld)
            {
                if (currentCatHealth.Value > minCatHealth)
                {
                    hasTakenDamage = true; // Prevent continuous damage
                    ApplyDamageServerRpc(-1); // Reduce health by 1 via ServerRpc
                    TeleportToSafeZone(); // Teleport to the safe zone
                    StartCoroutine(ResetDamageCooldown()); // Cooldown before allowing damage again
                }
            }
            else if (playerController.isWaterWorld)
            {
                Debug.Log("Player is safe in the Water World.");
            }
        }
    }

    [ServerRpc]
    private void ApplyDamageServerRpc(int damageAmount)
    {
        currentCatHealth.Value += damageAmount;
        currentCatHealth.Value = Mathf.Clamp(currentCatHealth.Value, minCatHealth, maxCatHealth);

        Debug.Log($"Player health updated to: {currentCatHealth.Value}");

        if (currentCatHealth.Value <= minCatHealth)
        {
            Debug.Log("Player has died!");
            // Handle death logic here
        }

        UpdateHealthClientRpc(currentCatHealth.Value); // Synchronize health across clients
    }

    [ClientRpc]
    private void UpdateHealthClientRpc(int updatedHealth)
    {
        currentCatHealth.Value = updatedHealth;

        if (HealthTextOnCanvas != null)
        {
            HealthTextOnCanvas.text = updatedHealth.ToString();
        }
    }

    public void TeleportToSafeZone()
    {
        GameObject spawnPoint = null;

        // Determine which spawn point to use based on the player's world
        if (playerController.isFireWorld)
        {
            spawnPoint = fireSpawnPoint;
        }
        else if (playerController.isPlantWorld)
        {
            spawnPoint = plantSpawnPoint;
        }
        else if (playerController.isMagicWorld)
        {
            spawnPoint = magicSpawnPoint;
        }
        else if (playerController.isWaterWorld)
        {
            spawnPoint = waterSpawnPoint;
        }

        if (spawnPoint == null)
        {
            Debug.LogError("Cannot teleport - no SpawnPoint found for the current world!");
            return;
        }

        Vector3 spawnPosition = spawnPoint.transform.position;

        Debug.Log($"Teleporting player to SpawnPoint at position: {spawnPosition}");

        // Set player position to the chosen SpawnPoint
        transform.position = spawnPosition;
        Debug.Log("Player position updated to: " + transform.position);
    }

    private IEnumerator ResetDamageCooldown()
    {
        yield return new WaitForSeconds(1.0f); // 1-second cooldown
        hasTakenDamage = false; // Allow damage to be taken again
        Debug.Log("Damage cooldown reset. Player can take damage again.");
    }

    private IEnumerator DelayedSpawnPointSearch()
    {
        Debug.Log("Starting delayed search for SpawnPoints...");
        yield return new WaitForSeconds(1.5f); // Wait for 1.5 seconds

        // Find all spawn points in the scene
        fireSpawnPoint = GameObject.Find("FireSpawnPoint");
        plantSpawnPoint = GameObject.Find("PlantSpawnPoint");
        waterSpawnPoint = GameObject.Find("WaterSpawnPoint");
        magicSpawnPoint = GameObject.Find("MagicSpawnPoint");

        if (fireSpawnPoint == null || plantSpawnPoint == null || waterSpawnPoint == null || magicSpawnPoint == null)
        {
            Debug.LogError("One or more SpawnPoints GameObjects not found in the scene after delay!");
        }
        else
        {
            Debug.Log($"SpawnPoints successfully found: Fire({fireSpawnPoint.transform.position}), Plant({plantSpawnPoint.transform.position}), Water({waterSpawnPoint.transform.position}), Magic({magicSpawnPoint.transform.position})");

            // Teleport players to their respective spawn points after delay
            TeleportToInitialSpawnPoints();
        }

        isSearchingForSpawnPoint = false; // Stop repeated searching
    }

    private void TeleportToInitialSpawnPoints()
    {
        if (fireSpawnPoint != null && playerController.isFireWorld)
        {
            transform.position = fireSpawnPoint.transform.position;
            Debug.Log("Player teleported to FireWorld spawn.");
        }
        else if (plantSpawnPoint != null && playerController.isPlantWorld)
        {
            transform.position = plantSpawnPoint.transform.position;
            Debug.Log("Player teleported to PlantWorld spawn.");
        }
        else if (waterSpawnPoint != null && playerController.isWaterWorld)
        {
            transform.position = waterSpawnPoint.transform.position;
            Debug.Log("Player teleported to WaterWorld spawn.");
        }
        else if (magicSpawnPoint != null && playerController.isMagicWorld)
        {
            transform.position = magicSpawnPoint.transform.position;
            Debug.Log("Player teleported to MagicWorld spawn.");
        }
    }
}
