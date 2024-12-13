using UnityEngine;

public class NetworkManagerController : MonoBehaviour
{
    private static NetworkManagerController _instance;

    void Awake()
    {
        // Check if an instance already exists
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject); // Destroy duplicate
            return;
        }

        // Assign the instance and make it persistent
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }
}