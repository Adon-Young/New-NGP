using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpawnPoints : MonoBehaviour
{
    public Transform[] spawnPoints;
    private HashSet<int> occupiedSpawnPoints = new HashSet<int>();//int for any spawn points used

    private void Awake()
    {
        spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint").Select(spawn => spawn.transform).ToArray();
    }


    public Transform GetAvailableSpawnPoint()
    {
        // Iterate through spawn points to find an available one
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (!occupiedSpawnPoints.Contains(i))
            {
                occupiedSpawnPoints.Add(i); // Mark this spawn point as occupied
                return spawnPoints[i]; // Return the transform of the spawn point
            }
        }

        // If no spawn points are available, return null
        Debug.LogWarning("No available spawn points!");
        return null;
    }

    // Optionally, you can have a method to free a spawn point if needed
    public void FreeSpawnPoint(int index)
    {
        occupiedSpawnPoints.Remove(index);
    }
}
