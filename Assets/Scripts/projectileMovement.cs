using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class ProjectileMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    public float projectileSpeed = 15.0f;

    // Set by the player controller on spawn
    public bool isSpriteFlipped;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        StartCoroutine(DestroyProjectileOverTime());

        // Determine direction based on the flip state
        Vector2 shootingDirection = isSpriteFlipped ? Vector2.left : Vector2.right;
        rb.velocity = shootingDirection * projectileSpeed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("TargetObject"))
        {
            GetComponent<NetworkObject>().Despawn(true); // despawning off the network
            Destroy(this.gameObject); // destroying locally
        }
    }

    private IEnumerator DestroyProjectileOverTime()
    {
        yield return new WaitForSeconds(2);
        GetComponent<NetworkObject>().Despawn(true); // despawning off the network
        Destroy(this.gameObject); // destroying locally
    }

    public void SetShootingDirection(Vector2 shootingDirection)
    {
        rb.velocity = shootingDirection * projectileSpeed;
    }

}
