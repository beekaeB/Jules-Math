using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 10f;
    private float xMin, xMax;

    [Header("Firing")]
    public GameObject projectilePrefab;
    public float fireRate = 0.5f;
    private float nextFire = 0f;

    [Header("Stats")]
    public int lives = 3;

    // Start is called before the first frame update
    void Start()
    {
        // Assuming the player sprite's width is negligible for screen bounds.
        // A more robust solution would account for the sprite's width.
        float camDistance = Vector3.Distance(transform.position, Camera.main.transform.position);
        Vector2 bottomCorner = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, camDistance));
        Vector2 topCorner = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, camDistance));
        xMin = bottomCorner.x;
        xMax = topCorner.x;
    }

    // Update is called once per frame
    void Update()
    {
        HandleMovement();
        HandleFiring();
    }

    void HandleMovement()
    {
        // Horizontal movement
        float horizontalInput = Input.GetAxis("Horizontal");
        transform.Translate(Vector3.right * horizontalInput * speed * Time.deltaTime);

        // Clamp position to screen bounds
        float newX = Mathf.Clamp(transform.position.x, xMin, xMax);
        transform.position = new Vector3(newX, transform.position.y, transform.position.z);
    }

    void HandleFiring()
    {
        // Using "Fire1" default input (e.g., left-click or Ctrl)
        if (Input.GetButton("Fire1") && Time.time > nextFire)
        {
            nextFire = Time.time + fireRate;
            Fire();
        }
    }

    void Fire()
    {
        // Instantiate the projectile at the player's position
        if (projectilePrefab != null)
        {
            Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        }
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

        // Notify the GameManager that the game is over.
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }

        // Deactivate the player object.
        gameObject.SetActive(false);
    }
}
