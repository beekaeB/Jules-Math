using UnityEngine;
using System.Collections;

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
    public Weapon equippedWeapon;
    public bool isDualFighter = false;

    private bool isCaptured = false;
    private Transform captor = null;
    private int currentMagazine;
    private bool isReloading = false;

    void Start()
    {
        float camDistance = Vector3.Distance(transform.position, Camera.main.transform.position);
        Vector2 bottomCorner = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, camDistance));
        Vector2 topCorner = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, camDistance));
        xMin = bottomCorner.x;
        xMax = topCorner.x;

        if (equippedWeapon == null)
        {
            EquipWeapon(new Weapon() { weaponName = "Default Blaster", baseMagazineSize = 20, baseReloadTime = 2f });
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
        HandleFiring();
        HandleReloading();
    }

    void HandleMovement()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        transform.Translate(Vector3.right * horizontalInput * speed * Time.deltaTime);
        float newX = Mathf.Clamp(transform.position.x, xMin, xMax);
        transform.position = new Vector3(newX, transform.position.y, transform.position.z);
    }

    void HandleFiring()
    {
        if (equippedWeapon == null || projectilePrefab == null) return;
        if (currentMagazine <= 0) return;

        float fireDelay = (equippedWeapon.FireRate > 0) ? 1f / equippedWeapon.FireRate : float.MaxValue;

        if (Input.GetButton("Fire1") && Time.time > nextFire)
        {
            nextFire = Time.time + fireDelay;
            Fire();
        }
    }

    void HandleReloading()
    {
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
                // Pass weapon stats to the thrown grenade-gun
                thrownScript.Initialize(currentMagazine, equippedWeapon.Damage, equippedWeapon.element);
            }
        }
        // Instantly reload
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

        GameObject projectileGO = Instantiate(projectilePrefab, position, Quaternion.identity);
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
        equippedWeapon = newWeapon;
        currentMagazine = equippedWeapon.MagazineSize;
        isReloading = false;
        Debug.Log($"Player equipped new weapon: {newWeapon.weaponName} | Ammo: {currentMagazine}");
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
}
