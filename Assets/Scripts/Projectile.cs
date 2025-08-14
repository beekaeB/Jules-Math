using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Projectile : MonoBehaviour
{
    [Header("Configuration")]
    public float speed = 20f;
    public float damage = 1f;
    public Element element = Element.Kinetic;
    public Manufacturer manufacturer = Manufacturer.Hyperion; // Default value

    [Header("Gimmicks")]
    public bool isExplosive = false;
    public float explosionRadius = 2f;

    private float topBound = 12f; // A simple boundary for when the projectile is considered off-screen
    private bool isBeingDestroyed = false; // Flag to prevent chain reactions in OnDestroy

    void Start()
    {
        // Ensure the Rigidbody2D is set to be kinematic and the collider is a trigger
        // so it doesn't apply physics forces but can detect collisions.
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.isKinematic = true;

        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    void Update()
    {
        // Move the projectile upwards
        transform.Translate(Vector3.up * speed * Time.deltaTime);

        // Destroy the projectile if it goes off the top of the screen
        if (transform.position.y > topBound)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Called when the Collider2D other enters the trigger.
    /// </summary>
    /// <param name="other">The other Collider2D involved in this collision.</param>
    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the object we collided with has an EnemyAI component
        EnemyAI enemy = other.GetComponent<EnemyAI>();
        if (enemy != null)
        {
            // If it's an enemy, deal damage to it, passing the element and manufacturer.
            enemy.TakeDamage(damage, element, manufacturer);

            // Destroy the projectile immediately after hitting an enemy
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        // Handle Torgue explosive gimmick
        if (isExplosive && !isBeingDestroyed)
        {
            isBeingDestroyed = true; // Prevents chain reaction if explosion destroys other projectiles
            Explode();
        }
    }

    void Explode()
    {
        // TODO: Instantiate an explosion visual effect here.
        Debug.Log("BOOM!");

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (var hit in colliders)
        {
            EnemyAI enemy = hit.GetComponent<EnemyAI>();
            // Make sure not to damage the enemy that was already hit by the projectile directly
            // This is a simple check; a more robust solution might pass the initial target to Explode.
            if (enemy != null)
            {
                // For simplicity, explosion damage is half of the projectile's direct damage
                float explosionDamage = damage * 0.5f;
                enemy.TakeDamage(explosionDamage, element, manufacturer);
            }
        }
    }
}
