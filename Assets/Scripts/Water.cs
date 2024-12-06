using UnityEngine;
using Unity.Netcode;

public class Water : NetworkBehaviour
{
    public NetworkVariable<bool> isRising = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private float riseSpeed = 0.5f; // Speed at which the water rises

    void Update()
    {
        // Handle water rising logic on the server
        if (IsServer)
        {
            if (isRising.Value)
            {
                transform.position += new Vector3(0, riseSpeed * Time.deltaTime, 0); // Raise the water on the Y-axis
                Debug.Log("Water is rising. Current position: " + transform.position);
            }
        }
    }

    // Toggle the water rising state on the server
    private void ToggleRising()
    {
        isRising.Value = !isRising.Value; // Toggle the rising state
        Debug.Log("Water rising state toggled: " + isRising.Value);
    }

    // Request the server to toggle the rising state (for client-side interactions)
    [ServerRpc(RequireOwnership = false)]
    public void ToggleRisingOnServerRpc()
    {
        ToggleRising();
    }

    // Handle trigger interactions
    private void OnTriggerEnter2D(Collider2D other)
    {
        Plantform plant = other.GetComponent<Plantform>();
        if (plant != null && plant.IsSeedling) // Check if the other object is a plant in seedling state
        {
            plant.GrowPlant(); // Trigger the plant growth
            Debug.Log("Water triggered plant growth.");
        }
    }
}
