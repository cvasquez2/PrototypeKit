using UnityEngine;
using System.Collections;

/// <summary>
/// Spawns enemy ships at regular intervals
/// Supports multiple enemy prefabs and dynamic spawn rate/max enemies
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject[] enemyPrefabs;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnInterval = 10f;
    [SerializeField] private int maxEnemies = 10;
    [SerializeField] private float spawnRadius = 100f;

    private Coroutine spawnCoroutine;

    [Header("Target Override")]
    [SerializeField] private Transform targetOverride; // Optional: force all spawned enemies to follow this

    private int currentEnemyCount = 0;

    /// <summary>
    /// Start the spawn loop coroutine
    /// </summary>
    private void Start()
    {
        spawnCoroutine = StartCoroutine(SpawnLoop());
    }

    /// <summary>
    /// Coroutine that spawns enemies at regular intervals
    /// </summary>
    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            // Spawn enemy if under max count
            if (currentEnemyCount < maxEnemies)
            {
                SpawnEnemy();
            }
        }
    }

    /// <summary>
    /// Change maximum number of enemies allowed
    /// </summary>
    /// <param name="max">New maximum enemy count</param>
    public void SetMaxEnemies(int max)
    {
        maxEnemies = max;
    }

    /// <summary>
    /// Change spawn interval and restart spawn loop
    /// </summary>
    /// <param name="interval">New spawn interval in seconds</param>
    public void SetSpawnInterval(float interval)
    {
        spawnInterval = interval;
        // Restart coroutine with new interval
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }
        spawnCoroutine = StartCoroutine(SpawnLoop());
    }

    /// <summary>
    /// Spawn a random enemy from the prefab array
    /// </summary>
    private void SpawnEnemy()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0) return;

        // Select random enemy prefab
        GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        if (prefab == null) return;

        // Spawn at random position within spawn radius
        Vector3 spawnPos = transform.position + Random.onUnitSphere * spawnRadius;
        GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.identity);
        currentEnemyCount++;

        // Track when this enemy dies so we can decrement the count
        Health health = enemy.GetComponent<Health>();
        if (health != null)
        {
            health.OnDeath += () =>
            {
                currentEnemyCount = Mathf.Max(0, currentEnemyCount - 1);
            };
        }
    }
}
