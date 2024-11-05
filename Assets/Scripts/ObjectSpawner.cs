using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ObjectSpawner : NetworkBehaviour
{

   [SerializeField] private GameObject spawnedObjectTransform;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (IsOwner)
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
               
                createBulletShotFromClientServerRpc(transform.position, transform.rotation);


            }
         
        }
     

    }


    [ServerRpc]
    private void createBulletShotFromClientServerRpc(Vector3 projectilePosition, Quaternion projectileRotation)
    {//simplified the position and rotation of the projectiles
        GameObject spawnedObject = Instantiate(spawnedObjectTransform, projectilePosition, projectileRotation);
        spawnedObject.GetComponent<NetworkObject>().Spawn(true);
    }


}
