using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StageManager : MonoBehaviour
{
    [Header("Stage Configuration")]
    public GameObject enemyPrefab;
    [Tooltip("A list of paths that can be used for this stage. A random one is chosen for each enemy.")]
    public List<PathAsset> enemyPaths;
    public int enemiesPerWave = 8;
    public float spawnInterval = 1f;
    public float timeBetweenWaves = 4f;

    private int waveCount = 0;
    private int enemiesAlive = 0;

    #region Unity & Event Lifecycle
    void OnEnable()
    {
        // Subscribe to the event to know when an enemy is destroyed.
        EnemyAI.OnEnemyDestroyed += HandleEnemyDestroyed;
    }

    void OnDisable()
    {
        // Unsubscribe to prevent errors and memory leaks.
        EnemyAI.OnEnemyDestroyed -= HandleEnemyDestroyed;
    }

    void Start()
    {
        if (enemyPrefab == null || enemyPaths == null || enemyPaths.Count == 0)
        {
            Debug.LogError("StageManager is not configured! Assign an enemy prefab and at least one PathAsset.", this);
            this.enabled = false;
            return;
        }

        StartCoroutine(WaveSpawnerCoroutine());
    }

    void HandleEnemyDestroyed(EnemyAI enemy)
    {
        // An enemy was destroyed, so decrement the counter of active enemies.
        if (enemiesAlive > 0)
        {
            enemiesAlive--;
            Debug.Log($"An enemy was destroyed. {enemiesAlive} enemies remaining in wave.");
        }
    }
    #endregion

    IEnumerator WaveSpawnerCoroutine()
    {
        // A simple starting delay
        yield return new WaitForSeconds(2f);

        while (true) // This loop runs forever to keep sending waves.
        {
            waveCount++;
            Debug.Log($"--- Starting Wave {waveCount} ---");

            yield return StartCoroutine(SpawnWaveCoroutine());

            Debug.Log($"Wave {waveCount} spawned. Waiting for all enemies to be destroyed.");
            // Wait until the wave is cleared
            yield return new WaitUntil(() => enemiesAlive == 0);

            Debug.Log($"Wave {waveCount} cleared! Preparing for next wave.");
            yield return new WaitForSeconds(timeBetweenWaves);
        }
    }

    IEnumerator SpawnWaveCoroutine()
    {
        enemiesAlive = enemiesPerWave;
        Debug.Log($"Spawning {enemiesAlive} enemies.");

        for (int i = 0; i < enemiesPerWave; i++)
        {
            if (enemyPaths.Count == 0) continue;

            // Pick a random path from the list for variety
            PathAsset selectedPath = enemyPaths[Random.Range(0, enemyPaths.Count)];

            GameObject enemyGO = Instantiate(enemyPrefab);
            EnemyAI enemyAI = enemyGO.GetComponent<EnemyAI>();

            if (enemyAI != null)
            {
                // Assign the path. The enemy will handle its own starting position and movement.
                enemyAI.AssignPath(selectedPath);
            }
            else
            {
                Debug.LogError("Spawned enemy prefab is missing an EnemyAI component!", enemyGO);
                HandleEnemyDestroyed(null); // Decrement count since this enemy is invalid
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }
}
