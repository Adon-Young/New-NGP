using Unity.Netcode;
using UnityEngine;
using TMPro;
using Unity.Collections;

public class LeaderboardManager : NetworkBehaviour
{
    public TMP_Text FireHealth;
    public TMP_Text WaterHealth;
    public TMP_Text PlantHealth;
    public TMP_Text MagicHealth;

    public TMP_Text FireColour;
    public TMP_Text WaterColour;
    public TMP_Text PlantColour;
    public TMP_Text MagiColour;

    public TMP_Text FireTag;
    public TMP_Text WaterTag;
    public TMP_Text PlantTag;
    public TMP_Text MagicTag;

    private NetworkVariable<FixedString32Bytes> fireColour = new NetworkVariable<FixedString32Bytes>(
        new FixedString32Bytes(),
        NetworkVariableReadPermission.Everyone, // Everyone can read
        NetworkVariableWritePermission.Server   // Only the server can write
    );

    private NetworkVariable<FixedString32Bytes> waterColour = new NetworkVariable<FixedString32Bytes>(
        new FixedString32Bytes(),
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private NetworkVariable<FixedString32Bytes> plantColour = new NetworkVariable<FixedString32Bytes>(
        new FixedString32Bytes(),
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private NetworkVariable<FixedString32Bytes> magiColour = new NetworkVariable<FixedString32Bytes>(
        new FixedString32Bytes(),
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private NetworkVariable<FixedString32Bytes> fireTag = new NetworkVariable<FixedString32Bytes>(
    new FixedString32Bytes(),
    NetworkVariableReadPermission.Everyone, // Everyone can read
    NetworkVariableWritePermission.Server   // Only the server can write
);

    private NetworkVariable<FixedString32Bytes> waterTag = new NetworkVariable<FixedString32Bytes>(
        new FixedString32Bytes(),
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private NetworkVariable<FixedString32Bytes> plantTag = new NetworkVariable<FixedString32Bytes>(
        new FixedString32Bytes(),
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private NetworkVariable<FixedString32Bytes> magicTag = new NetworkVariable<FixedString32Bytes>(
        new FixedString32Bytes(),
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );





    float waterCatHealth;
    float fireCatHealth;
    float plantCatHealth;
    float magicCatHealth;

    void Update()
    {




        // Only the server processes leaderboard updates
        if (!IsServer) return;

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");



        foreach (GameObject player in players)
        {
            PlayerCollision playerCollision = player.GetComponent<PlayerCollision>();
            CatHealth catHealth = player.GetComponent<CatHealth>();



            if (playerCollision != null && catHealth != null)
            {


                switch (playerCollision.playerType.Value)
                {
                    case PlayerCollision.PlayerType.Water:
                        waterCatHealth = catHealth.currentCatHealth.Value;
                        waterColour.Value = new FixedString32Bytes("BLUE");
                        waterTag.Value = new FixedString32Bytes("WATER");

                        break;
                    case PlayerCollision.PlayerType.Fire:
                        fireCatHealth = catHealth.currentCatHealth.Value;
                        fireColour.Value = new FixedString32Bytes("RED");
                        fireTag.Value = new FixedString32Bytes("FIRE");

                        break;
                    case PlayerCollision.PlayerType.Plant:
                        plantCatHealth = catHealth.currentCatHealth.Value;
                        plantColour.Value = new FixedString32Bytes("GREEN");
                        plantTag.Value = new FixedString32Bytes("PLANT");

                        break;
                    case PlayerCollision.PlayerType.Magic:
                        magicCatHealth = catHealth.currentCatHealth.Value;
                        magiColour.Value = new FixedString32Bytes("PURPLE");
                        magicTag.Value = new FixedString32Bytes("MAGIC");
                        break;
                }
            }

        }



        UpdateLeaderboardUIClientRpc(
        waterCatHealth, waterColour.Value.ToString(), waterTag.Value.ToString(), fireCatHealth, fireColour.Value.ToString(), fireTag.Value.ToString(), plantCatHealth, plantColour.Value.ToString(), plantTag.Value.ToString(), magicCatHealth, magiColour.Value.ToString(), magicTag.Value.ToString()
    );
    }

    [ClientRpc]
    private void UpdateLeaderboardUIClientRpc(
        float waterHealth, string waterColour, string waterTag, // Added waterTag
        float fireHealth, string fireColour, string fireTag, // Added fireTag
        float plantHealth, string plantColour, string plantTag, // Added plantTag
        float magicHealth, string magicColour, string magicTag // Added magicTag
    )
    {
        // Update health
        WaterHealth.text = $"{waterHealth}";
        FireHealth.text = $"{fireHealth}";
        PlantHealth.text = $"{plantHealth}";
        MagicHealth.text = $"{magicHealth}";

        // Update colors
        FireColour.text = $"{fireColour}";
        WaterColour.text = $"{waterColour}";
        PlantColour.text = $"{plantColour}";
        MagiColour.text = $"{magicColour}";

        // Update tags (display them in the appropriate UI fields)
        FireTag.text = $"{fireTag}";
        WaterTag.text = $"{waterTag}";
        PlantTag.text = $"{plantTag}";
        MagicTag.text = $"{magicTag}";
    }
}