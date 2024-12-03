using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class ProjectileMovement : NetworkBehaviour
{
    private Rigidbody2D rb;
    public float projectileSpeed = 15.0f;

    // NetworkVariable to sync the velocity across clients
    private NetworkVariable<Vector2> velocity = new NetworkVariable<Vector2>();

    // Set by the player controller on spawn
    public bool isSpriteFlipped;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        StartCoroutine(DestroyProjectileOverTime());

        if (IsServer)
        {
            // Determine direction based on the flip state
            Vector2 shootingDirection = isSpriteFlipped ? Vector2.left : Vector2.right;
            velocity.Value = shootingDirection * projectileSpeed;
        }
    }

    private void Update()
    {
        if (IsClient)
        {
            // Ensure the client updates the Rigidbody's velocity based on the NetworkVariable
            rb.velocity = velocity.Value;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // When projectile collides with the "TargetObject" (wall)
        if (collision.gameObject.CompareTag("TargetObject"))
        {
            // Despawn the projectile off the network and destroy it locally
            GetComponent<NetworkObject>().Despawn(true);
            Destroy(this.gameObject);

            // Destroy the wall and trigger the wall switch event
            collision.gameObject.GetComponent<NetworkObject>().Despawn(true);
            Destroy(collision.gameObject);  // Destroy the wall locally

            // Trigger event for wall destruction
            WallManagement.Instance.ActivateFloatingWall();
        }
    }

    private IEnumerator DestroyProjectileOverTime()
    {
        yield return new WaitForSeconds(2);
        GetComponent<NetworkObject>().Despawn(true); // Despawn the projectile from the network
        Destroy(this.gameObject); // Destroy it locally
    }

    // Call this function to set the direction from the server
    public void SetVelocity(Vector2 newVelocity)
    {
        if (IsServer)
        {
            velocity.Value = newVelocity;
        }
    }
}
