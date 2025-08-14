using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    // Defines the behavioral state of the enemy.
    public enum EnemyState { ENTERING, IN_FORMATION, DIVING, DEAD }
    public EnemyState currentState;

    [Header("Stats")]
    public HealthType healthType = HealthType.Flesh;
    public int health = 1;
    public int points = 10;

    [Header("Movement")]
    public PathAsset pathAsset;
    public float movementSpeed = 5f;
    public float diveSpeed = 4f;
    [Tooltip("The base time the enemy will wait in formation before diving.")]
    public float timeInFormation = 2.5f;

    [Header("Loot")]
    [Tooltip("The prefab for the weapon pickup object.")]
    public GameObject weaponPickupPrefab;
    [Tooltip("The probability (0 to 1) that this enemy will drop loot upon death.")]
    [Range(0f, 1f)]
    public float lootDropChance = 0.1f;

    // Event that is broadcast when this enemy is destroyed.
    public static event System.Action<EnemyAI> OnEnemyDestroyed;

    private int waypointIndex = 0;
    private float bottomBound = -7f; // Off-screen boundary

    protected virtual void Start()
    {
        // This is a failsafe. The path should be assigned by the StageManager upon spawning.
        if (pathAsset != null)
        {
            StartPathing();
        }
        else
        {
            // If no path, go straight to diving.
            currentState = EnemyState.DIVING;
        }
    }

    protected virtual void Update()
    {
        // Only perform the dive behavior when in the DIVING state.
        if (currentState == EnemyState.DIVING)
        {
            transform.Translate(Vector3.down * diveSpeed * Time.deltaTime);
            if (transform.position.y < bottomBound)
            {
                Destroy(gameObject);
            }
        }
    }

    /// <summary>
    /// Assigns a path to this enemy and starts its entrance sequence.
    /// </summary>
    public void AssignPath(PathAsset newPath)
    {
        pathAsset = newPath;
        StartPathing();
    }

    private void StartPathing()
    {
        // Set initial position to the start of the path
        if (pathAsset.waypoints.Count > 0)
        {
            transform.position = pathAsset.waypoints[0];
            waypointIndex = 1; // Start moving towards the second waypoint
            currentState = EnemyState.ENTERING;
            StartCoroutine(FollowPathCoroutine());
        }
        else
        {
            Debug.LogWarning("Path Asset is empty. Defaulting to IN_FORMATION.", this);
            currentState = EnemyState.IN_FORMATION;
            StartCoroutine(WaitInFormationCoroutine());
        }
    }

    IEnumerator FollowPathCoroutine()
    {
        while (waypointIndex < pathAsset.waypoints.Count)
        {
            Vector3 targetPosition = pathAsset.waypoints[waypointIndex];
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, movementSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                waypointIndex++;
            }
            yield return null;
        }

        // Path complete, now in formation
        currentState = EnemyState.IN_FORMATION;
        StartCoroutine(WaitInFormationCoroutine());
    }

    protected virtual IEnumerator WaitInFormationCoroutine()
    {
        // Wait for a slightly randomized amount of time before diving
        yield return new WaitForSeconds(Random.Range(timeInFormation * 0.8f, timeInFormation * 1.2f));
        currentState = EnemyState.DIVING;
    }

    public void TakeDamage(float damage, Element damageElement, Manufacturer manufacturer)
    {
        if (currentState == EnemyState.DEAD) return;

        // --- Elemental Damage Calculation ---
        float multiplier = 1.0f; // Default multiplier
        switch (healthType)
        {
            case HealthType.Flesh:
                if (damageElement == Element.Incendiary) multiplier = 2.0f;
                else if (damageElement == Element.Shock) multiplier = 0.5f;
                break;
            case HealthType.Shield:
                if (damageElement == Element.Shock) multiplier = 2.0f;
                else if (damageElement == Element.Corrosive) multiplier = 0.5f;
                break;
            case HealthType.Armor:
                if (damageElement == Element.Corrosive) multiplier = 2.0f;
                else if (damageElement == Element.Incendiary) multiplier = 0.5f;
                break;
        }

        int finalDamage = Mathf.CeilToInt(damage * multiplier);
        health -= finalDamage;

        Debug.Log($"Enemy took {finalDamage} ({damageElement}) damage from a {manufacturer} weapon. Health: {health}");

        if (health <= 0)
        {
            // Handle Jakobs Gimmick: Ricochet on kill
            if (manufacturer == Manufacturer.Jakobs)
            {
                Ricochet(damage, damageElement, manufacturer);
            }
            Die();
        }
    }

    private void Ricochet(float originalDamage, Element originalElement, Manufacturer originalManufacturer)
    {
        Debug.Log("JAKOBS: Ricochet!");
        Collider2D[] nearbyEnemies = Physics2D.OverlapCircleAll(transform.position, 5f); // 5f is ricochet range

        EnemyAI closestEnemy = null;
        float minDistance = float.MaxValue;

        foreach (var col in nearbyEnemies)
        {
            EnemyAI enemy = col.GetComponent<EnemyAI>();
            if (enemy != null && enemy != this && enemy.currentState != EnemyState.DEAD)
            {
                float distance = Vector2.Distance(transform.position, enemy.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestEnemy = enemy;
                }
            }
        }

        if (closestEnemy != null)
        {
            // Ricochet deals half damage
            closestEnemy.TakeDamage(originalDamage * 0.5f, originalElement, originalManufacturer);
        }
    }

    protected virtual void Die()
    {
        if (currentState == EnemyState.DEAD) return;
        currentState = EnemyState.DEAD;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(points);
        }

        // Notify any listeners (like the StageManager) that this enemy is gone.
        OnEnemyDestroyed?.Invoke(this);

        // Handle loot drop
        TryDropLoot();

        Destroy(gameObject);
    }

    private void TryDropLoot()
    {
        if (Random.value <= lootDropChance)
        {
            if (LootManager.Instance != null && weaponPickupPrefab != null)
            {
                Weapon generatedWeapon = LootManager.Instance.GenerateWeapon();
                GameObject pickup = Instantiate(weaponPickupPrefab, transform.position, Quaternion.identity);
                WeaponPickup pickupScript = pickup.GetComponent<WeaponPickup>();
                if (pickupScript != null)
                {
                    pickupScript.Initialize(generatedWeapon);
                }
            }
            else if (weaponPickupPrefab == null)
            {
                Debug.LogWarning("Enemy has no weaponPickupPrefab assigned.", this);
            }
        }
    }

    void OnDestroy()
    {
        // This ensures that if the enemy is destroyed for any reason (e.g., going off-screen),
        // it still notifies the StageManager.
        if (currentState != EnemyState.DEAD)
        {
            OnEnemyDestroyed?.Invoke(this);
        }
    }
}
