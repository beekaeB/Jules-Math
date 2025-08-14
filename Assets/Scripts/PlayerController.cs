using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 10f;
    private float xMin, xMax;

    [Header("Firing")]
    public GameObject projectilePrefab;
    public GameObject thrownWeaponPrefab; // For Tediore reloads
    private float nextFire = 0f;

    [Header("Player State")]
    public int lives = 3;
    public List<Weapon> weapons = new List<Weapon>();
    public int inventorySize = 3;
    public bool isDualFighter = false;

    // Read-only property to get the currently equipped weapon
    public Weapon equippedWeapon => weapons.Any() ? weapons[currentWeaponIndex] : null;

    // --- Private State ---
    private int currentWeaponIndex = 0;
    private bool isCaptured = false;
    private Transform captor = null;
    private int currentMagazine;
    private bool isReloading = false;

    // Gimmick State
    private float timeHeld = 0f;
    private float currentFireRateBonus = 0f;
    private float currentAccuracyBonus = 0f;

    void Start()
    {
        float camDistance = Vector3.Distance(transform.position, Camera.main.transform.position);
        Vector2 bottomCorner = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, camDistance));
        Vector2 topCorner = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, camDistance));
        xMin = bottomCorner.x;
        xMax = topCorner.x;

        if (!weapons.Any())
        {
            // Add and equip a default 'starter' weapon if inventory is empty.
            var starterWeapon = new Weapon() { weaponName = "Default Blaster", baseMagazineSize = 20, baseReloadTime = 2f };
            weapons.Add(starterWeapon);
            SwitchWeapon(0);
        }
    }

    void Update()
    {
        if (isCaptured || isReloading)
        {
            if (isCaptured && captor != null)
            {
                transform.position = captor.position + Vector3.down;
            }
            return;
        }

        HandleMovement();
        HandleGimmicks();
        HandleFiring();
        HandleReloading();
        HandleWeaponSwitching();
    }

    #region Handlers
    void HandleMovement()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        transform.Translate(Vector3.right * horizontalInput * speed * Time.deltaTime);
        float newX = Mathf.Clamp(transform.position.x, xMin, xMax);
        transform.position = new Vector3(newX, transform.position.y, transform.position.z);
    }

    void HandleGimmicks()
    {
        if (equippedWeapon == null) return;

        if (Input.GetButton("Fire1"))
        {
            timeHeld += Time.deltaTime;
        }
        else
        {
            timeHeld = 0f;
            currentFireRateBonus = 0f;
            currentAccuracyBonus = 0f;
        }

        if (equippedWeapon.manufacturer == Manufacturer.Vladof)
        {
            currentFireRateBonus = timeHeld * equippedWeapon.FireRateRamp;
        }

        if (equippedWeapon.manufacturer == Manufacturer.Hyperion)
        {
            currentAccuracyBonus = timeHeld * equippedWeapon.AccuracyRamp;
        }
    }

    void HandleFiring()
    {
        if (equippedWeapon == null || projectilePrefab == null) return;
        if (currentMagazine <= 0) return;

        float finalFireRate = equippedWeapon.FireRate + currentFireRateBonus;
        float fireDelay = (finalFireRate > 0) ? 1f / finalFireRate : float.MaxValue;

        if (Input.GetButton("Fire1") && Time.time > nextFire)
        {
            nextFire = Time.time + fireDelay;
            Fire();
        }
    }

    void HandleReloading()
    {
        if (equippedWeapon == null) return;
        if (Input.GetKeyDown(KeyCode.R) && currentMagazine < equippedWeapon.MagazineSize)
        {
            if (equippedWeapon.manufacturer == Manufacturer.Tediore)
            {
                TedioreReload();
            }
            else
            {
                StartCoroutine(ReloadCoroutine());
            }
        }
    }

    void HandleWeaponSwitching()
    {
        // Use Q and E to cycle through weapons
        if (Input.GetKeyDown(KeyCode.E))
        {
            SwitchWeapon((currentWeaponIndex + 1) % weapons.Count);
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            int newIndex = currentWeaponIndex - 1;
            if (newIndex < 0) newIndex = weapons.Count - 1;
            SwitchWeapon(newIndex);
        }
    }
    #endregion

    #region Actions
    IEnumerator ReloadCoroutine()
    {
        isReloading = true;
        Debug.Log("Reloading...");
        yield return new WaitForSeconds(equippedWeapon.ReloadTime);
        currentMagazine = equippedWeapon.MagazineSize;
        isReloading = false;
        Debug.Log("Reload complete! Ammo: " + currentMagazine);
    }

    void TedioreReload()
    {
        Debug.Log("Tediore reload: Throwing weapon!");
        if (thrownWeaponPrefab != null)
        {
            GameObject thrownGO = Instantiate(thrownWeaponPrefab, transform.position, Quaternion.identity);
            ThrownWeapon thrownScript = thrownGO.GetComponent<ThrownWeapon>();
            if (thrownScript != null)
            {
                thrownScript.Initialize(currentMagazine, equippedWeapon.Damage, equippedWeapon.element);
            }
        }
        currentMagazine = equippedWeapon.MagazineSize;
    }

    void Fire()
    {
        if (isDualFighter)
        {
            SpawnProjectile(transform.position + new Vector3(-0.25f, 0, 0));
            SpawnProjectile(transform.position + new Vector3(0.25f, 0, 0));
        }
        else
        {
            SpawnProjectile(transform.position);
        }
    }

    void SpawnProjectile(Vector3 position)
    {
        if (projectilePrefab == null || currentMagazine <= 0) return;

        currentMagazine--;

        float currentSpread = equippedWeapon.baseSpread - currentAccuracyBonus;
        float spread = Random.Range(-currentSpread, currentSpread) * 0.5f;
        Quaternion rotation = Quaternion.Euler(0, 0, spread);

        GameObject projectileGO = Instantiate(projectilePrefab, position, rotation);
        Projectile projectile = projectileGO.GetComponent<Projectile>();

        if (projectile != null)
        {
            projectile.damage = equippedWeapon.Damage;
            projectile.element = equippedWeapon.element;
            projectile.manufacturer = equippedWeapon.manufacturer;

            if (equippedWeapon.manufacturer == Manufacturer.Torgue)
            {
                projectile.isExplosive = true;
            }
        }
    }

    public void EquipWeapon(Weapon newWeapon)
    {
        if (weapons.Count < inventorySize)
        {
            weapons.Add(newWeapon);
            SwitchWeapon(weapons.Count - 1);
        }
        else
        {
            // Replace current weapon if inventory is full
            weapons[currentWeaponIndex] = newWeapon;
            SwitchWeapon(currentWeaponIndex);
        }
    }

    private void SwitchWeapon(int newIndex)
    {
        if (newIndex < 0 || newIndex >= weapons.Count) return;

        currentWeaponIndex = newIndex;
        currentMagazine = equippedWeapon.MagazineSize;
        isReloading = false;

        // Reset gimmick stats on weapon swap
        timeHeld = 0f;
        currentFireRateBonus = 0f;
        currentAccuracyBonus = 0f;

        Debug.Log($"Switched to weapon: {equippedWeapon.weaponName} | Ammo: {currentMagazine}");
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
        isCaptured = true;
        captor = captorTransform;
        Debug.Log("Player has been captured!");
    }

    public void OnRescue()
    {
        isCaptured = false;
        captor = null;
        isDualFighter = true;
        Debug.Log("Player has been rescued! Dual Fighter mode activated!");
    }
    #endregion
}
