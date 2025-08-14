using UnityEngine;

/// <summary>
/// Represents a Tediore weapon that has been thrown as a grenade upon reloading.
/// </summary>
public class ThrownWeapon : MonoBehaviour
{
    [Header("Behavior")]
    public float speed = 7f;
    public float rotationSpeed = 450f;
    public float lifetime = 1.2f;
    public float explosionRadius = 3f;

    // Stats passed from the thrown weapon
    private float damagePerBullet;
    private int bulletsLeft;
    private Element element;

    /// <summary>
    /// Initializes the thrown weapon with stats from the gun that was thrown.
    /// </summary>
    public void Initialize(int remainingAmmo, float baseDamage, Element weaponElement)
    {
        bulletsLeft = remainingAmmo;
        // Let's make the damage per bullet a fraction of the weapon's base damage
        damagePerBullet = baseDamage * 0.5f;
        element = weaponElement;

        // Automatically destroy the object after its lifetime expires, which will trigger OnDestroy()
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // Simple upward movement with a spin
        transform.Translate(Vector3.up * speed * Time.deltaTime, Space.World);
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }

    // This is called when the object is destroyed, either by lifetime expiring or other means.
    void OnDestroy()
    {
        Explode();
    }

    void Explode()
    {
        // In a real game, you would instantiate an explosion visual effect here.
        Debug.Log("Tediore weapon exploded!");

        // Damage is based on the number of bullets left in the magazine.
        float totalDamage = bulletsLeft * damagePerBullet;
        if (totalDamage <= 0) return;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (var hit in colliders)
        {
            EnemyAI enemy = hit.GetComponent<EnemyAI>();
            if (enemy != null)
            {
                // The explosion itself is just Tediore "type" damage, not Jakobs, etc.
                // This prevents infinite ricochets if a Jakobs weapon were thrown.
                enemy.TakeDamage(totalDamage, element, Manufacturer.Tediore);
            }
        }
    }
}
