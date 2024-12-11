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
    public Text HealthTextOnCanvas;
    private NewPlayerController playerController;
    public GameObject safeSpawnArea;

    private bool hasTakenDamage = false; // Prevent continuous damage
    private bool hasTeleported = false; // Check if the player has been teleported

    void Start()
    {
        playerController = GetComponent<NewPlayerController>();
        safeSpawnArea = GameObject.Find("SpawnPoint");
        if (playerController == null)
        {
            Debug.LogError("NewPlayerController script is missing on the GameObject!");
        }
        if (safeSpawnArea == null)
        {
            Debug.LogError("SpawnPoint GameObject not found! Check the name or ensure it exists in the scene.");
        }
        else
        {
            Debug.Log($"SpawnPoint found: {safeSpawnArea.name} at position {safeSpawnArea.transform.position}");
        }

    }

    void Update()
    {
        if (!IsOwner) return; // Only the owning client handles local logic

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
            if (playerController.isFireWorld || playerController.isPlantWorld || playerController.isMagicWorld)
            {
                if (currentCatHealth.Value > minCatHealth)
                {
                    hasTakenDamage = true; // Prevent continuous damage
                    ApplyDamage(-1); // Reduce health by 1
                    TeleportToSafeZone(); // Teleport to a safe location
                    StartCoroutine(ResetDamageCooldown()); // Reset cooldown after a delay
                }
            }
         
        }
    }

    private void ApplyDamage(int damageAmount)
    {
        if (!IsServer)
        {
            UpdateCatHealthServerRpc(damageAmount, OwnerClientId);
        }
        else
        {
            currentCatHealth.Value += damageAmount;
            currentCatHealth.Value = Mathf.Clamp(currentCatHealth.Value, minCatHealth, maxCatHealth);

            if (currentCatHealth.Value <= minCatHealth && !catHasDied.Value)
            {
                catHasDied.Value = true;
                Debug.Log("Cat Has Purrrrished-Must reset level/game");
                // Handle death logic here
            }
        }
    }

    private void TeleportToSafeZone()
    {
        if (!IsServer) return;

        if (safeSpawnArea == null)
        {
            Debug.LogError("No SpawnPoint object found in the scene!");
            return;
        }

        Vector3 spawnPosition = safeSpawnArea.transform.position;

        if (!hasTeleported || transform.position != spawnPosition)
        {
            hasTeleported = true; // Mark teleportation as complete
            TeleportPlayerServerRpc(spawnPosition);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void TeleportPlayerServerRpc(Vector3 newPosition)
    {
        transform.position = newPosition; // Update position on the server
        TeleportPlayerClientRpc(newPosition); // Sync position across all clients
    }

    [ClientRpc]
    private void TeleportPlayerClientRpc(Vector3 newPosition)
    {
        transform.position = newPosition; // Update position on the client
        hasTeleported = true; // Ensure the teleportation status is updated on the client
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateCatHealthServerRpc(int addValue, ulong clientId)
    {
        currentCatHealth.Value += addValue;
        currentCatHealth.Value = Mathf.Clamp(currentCatHealth.Value, minCatHealth, maxCatHealth);

        if (currentCatHealth.Value <= minCatHealth && !catHasDied.Value)
        {
            catHasDied.Value = true;
            Debug.Log("Player has died!");
        }

        UpdateCatHealthClientRpc(currentCatHealth.Value);
    }

    [ClientRpc]
    private void UpdateCatHealthClientRpc(int newHealth)
    {
        if (HealthTextOnCanvas != null)
        {
            HealthTextOnCanvas.text = newHealth.ToString();
        }
    }

    private IEnumerator ResetDamageCooldown()
    {
        yield return new WaitForSeconds(1.0f); // 1-second cooldown
        hasTakenDamage = false;
        hasTeleported = false; // Allow teleportation to happen again
    }
}
