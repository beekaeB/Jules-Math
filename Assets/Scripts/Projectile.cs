using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Projectile : MonoBehaviour
{
    [Header("Configuration")]
    public float speed = 20f;
    public float damage = 1f; // Damage is now a float to match the Weapon's stats

    private float topBound = 12f; // A simple boundary for when the projectile is considered off-screen

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
            // If it's an enemy, deal damage to it.
            // We cast the float damage to an int because the enemy's health is an integer.
            enemy.TakeDamage((int)damage);

            // Destroy the projectile immediately after hitting an enemy
            Destroy(gameObject);
        }
    }
}
