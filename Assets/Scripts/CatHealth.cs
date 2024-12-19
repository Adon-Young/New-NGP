using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class CatHealth : NetworkBehaviour
{
    public NetworkVariable<int> currentCatHealth = new NetworkVariable<int>(3,NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private int maxCatHealth = 3;
    private int minCatHealth = 0;
    public Text HealthTextOnCanvas;

    private GameObject fireSpawnPoint;
    private GameObject plantSpawnPoint;
    private GameObject waterSpawnPoint;
    private GameObject magicSpawnPoint;
    private bool hasTakenDamage = false;
    private bool isSearchingForSpawnPoint = true;

    private NewPlayerController playerController;

    void Start()
    {
        playerController = GetComponent<NewPlayerController>();
        if (playerController == null)
        {
            Debug.LogError("NewPlayerController script is missing on the GameObject!");
        }

        // Subscribe to the OnValueChanged event to update UI when health changes
        currentCatHealth.OnValueChanged += UpdateHealthOnCanvas;

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
    }

    private void UpdateHealthOnCanvas(int previousHealth, int newHealth)
    {
        if (HealthTextOnCanvas != null)
        {
            HealthTextOnCanvas.text = newHealth.ToString();
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
                    hasTakenDamage = true;
                    ApplyDamageServerRpc(-1);
                    TeleportToSafeZone();
                    StartCoroutine(ResetDamageCooldown());
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
    }

    public void TeleportToSafeZone()
    {
        GameObject spawnPoint = null;

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
        transform.position = spawnPosition;
        Debug.Log("Player position updated to: " + transform.position);
    }

    private IEnumerator ResetDamageCooldown()
    {
        yield return new WaitForSeconds(1.0f);
        hasTakenDamage = false;
        Debug.Log("Damage cooldown reset. Player can take damage again.");
    }

    private IEnumerator DelayedSpawnPointSearch()
    {
        Debug.Log("Starting delayed search for SpawnPoints...");
        yield return new WaitForSeconds(1.5f);

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
            TeleportToInitialSpawnPoints();
        }

        isSearchingForSpawnPoint = false;
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
