using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    // Defines the behavioral state of the enemy.
    public enum EnemyState { ENTERING, IN_FORMATION, DIVING, DEAD }
    public EnemyState currentState;

    [Header("Stats")]
    public int health = 1;
    public int points = 10;

    [Header("Movement")]
    public PathAsset pathAsset;
    public float movementSpeed = 5f;
    public float diveSpeed = 4f;
    [Tooltip("The base time the enemy will wait in formation before diving.")]
    public float timeInFormation = 2.5f;

    // Event that is broadcast when this enemy is destroyed.
    public static event System.Action<EnemyAI> OnEnemyDestroyed;

    private int waypointIndex = 0;
    private float bottomBound = -7f; // Off-screen boundary

    void Start()
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

    void Update()
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

    IEnumerator WaitInFormationCoroutine()
    {
        // Wait for a slightly randomized amount of time before diving
        yield return new WaitForSeconds(Random.Range(timeInFormation * 0.8f, timeInFormation * 1.2f));
        currentState = EnemyState.DIVING;
    }

    public void TakeDamage(int damage)
    {
        if (currentState == EnemyState.DEAD) return;

        health -= damage;
        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (currentState == EnemyState.DEAD) return;
        currentState = EnemyState.DEAD;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(points);
        }

        // Notify any listeners (like the StageManager) that this enemy is gone.
        OnEnemyDestroyed?.Invoke(this);

        // Placeholder for explosion effects, loot drops, etc.

        Destroy(gameObject);
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
