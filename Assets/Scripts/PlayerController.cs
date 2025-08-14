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
        // Instantiate the projectile
        GameObject projectileGO = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        Projectile projectile = projectileGO.GetComponent<Projectile>();

        // Pass the weapon's damage to the projectile
        if (projectile != null)
        {
            projectile.damage = (int)equippedWeapon.Damage; // Casting float to int for the projectile
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
}
