using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 10f;
    private float xMin, xMax;

    [Header("Firing")]
    public GameObject projectilePrefab;
    private float nextFire = 0f;

    [Header("Player State")]
    public int lives = 3;
    public Weapon equippedWeapon;
    public bool isDualFighter = false;

    private bool isCaptured = false;
    private Transform captor = null;

    void Start()
    {
        // Calculate screen boundaries for player movement
        float camDistance = Vector3.Distance(transform.position, Camera.main.transform.position);
        Vector2 bottomCorner = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, camDistance));
        Vector2 topCorner = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, camDistance));
        xMin = bottomCorner.x;
        xMax = topCorner.x;

        // Create and equip a default 'starter' weapon if none is assigned.
        if (equippedWeapon == null)
        {
            EquipWeapon(new Weapon() { weaponName = "Default Blaster" });
        }
    }

    void Update()
    {
        if (isCaptured)
        {
            // If captured, follow the captor
            if (captor != null)
            {
                transform.position = captor.position + Vector3.down; // Follow slightly below the captor
            }
            return; // No input while captured
        }

        HandleMovement();
        HandleFiring();
    }

    void HandleMovement()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        transform.Translate(Vector3.right * horizontalInput * speed * Time.deltaTime);

        // Clamp position to keep the player within the screen bounds
        float newX = Mathf.Clamp(transform.position.x, xMin, xMax);
        transform.position = new Vector3(newX, transform.position.y, transform.position.z);
    }

    void HandleFiring()
    {
        // Can't fire if there's no weapon or projectile prefab.
        if (equippedWeapon == null || projectilePrefab == null) return;

        // Fire rate is measured in shots per second. We calculate the delay between shots.
        // We also check that FireRate is not zero to avoid a division by zero error.
        float fireDelay = (equippedWeapon.FireRate > 0) ? 1f / equippedWeapon.FireRate : float.MaxValue;

        if (Input.GetButton("Fire1") && Time.time > nextFire)
        {
            nextFire = Time.time + fireDelay;
            Fire();
        }
    }

    void Fire()
    {
        // Define spawn positions for projectiles
        Vector3 leftSpawn = transform.position + new Vector3(-0.25f, 0, 0);
        Vector3 rightSpawn = transform.position + new Vector3(0.25f, 0, 0);

        if (isDualFighter)
        {
            // Fire from two positions
            SpawnProjectile(leftSpawn);
            SpawnProjectile(rightSpawn);
        }
        else
        {
            // Fire from the center
            SpawnProjectile(transform.position);
        }
    }

    void SpawnProjectile(Vector3 position)
    {
        if (projectilePrefab == null) return;

        GameObject projectileGO = Instantiate(projectilePrefab, position, Quaternion.identity);
        Projectile projectile = projectileGO.GetComponent<Projectile>();

        if (projectile != null)
        {
            // Pass the weapon's stats to the projectile
            projectile.damage = equippedWeapon.Damage;
            projectile.element = equippedWeapon.element;
            projectile.manufacturer = equippedWeapon.manufacturer;

            // Apply Torgue gimmick
            if (equippedWeapon.manufacturer == Manufacturer.Torgue)
            {
                projectile.isExplosive = true;
            }
        }
    }

    /// <summary>
    /// Equips a new weapon, replacing the old one.
    /// </summary>
    public void EquipWeapon(Weapon newWeapon)
    {
        equippedWeapon = newWeapon;
        Debug.Log($"Player equipped new weapon: {newWeapon.weaponName}" + System.Environment.NewLine + newWeapon.ToString());
    }

    public void LoseLife()
    {
        lives--;
        if (lives <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Player has died!");
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
        gameObject.SetActive(false);
    }

    public void OnCapture(Transform captorTransform)
    {
        if (isCaptured) return;

        Debug.Log("Player has been captured!");
        isCaptured = true;
        captor = captorTransform;
        // In a real game, you might also want to disable the player's collider here.
    }

    public void OnRescue()
    {
        Debug.Log("Player has been rescued! Dual Fighter mode activated!");
        isCaptured = false;
        captor = null;
        isDualFighter = true;
        // You might want to start a timer to disable dual fighter mode after a while.
    }
}
