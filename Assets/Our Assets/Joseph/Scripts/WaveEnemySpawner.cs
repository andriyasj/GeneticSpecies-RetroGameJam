using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Wave
{
    public int enemyCount;
    public float timeBetweenSpawns = 1f;
    public float timeToNextWave = 5f;
}

public class WaveEnemySpawner : MonoBehaviour
{
    [Header("Wave Settings")]
    [SerializeField] private Wave[] waves;
    [SerializeField] private GameObject enemyPrefab;
    
    [Header("Spawn Points")]
    [SerializeField] private Transform[] allSpawnPoints; // All red spawn locations
    [SerializeField] private int spawnPointsPerWave = 3; // Number of closest spawns to use
    
    [Header("Player Reference")]
    [SerializeField] private Transform player;
    
    private int currentWaveIndex = 0;
    private int enemiesLeftToSpawn = 0;
    private int enemiesAlive = 0;
    private Transform[] activeSpawnPoints; // The 3 closest spawns for current wave
    private bool isSpawning = false;

    void Start()
    {
        // Find player if not assigned
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        }
        
        StartWave();
    }

    void Update()
    {
        // Check if wave is complete
        if (!isSpawning && enemiesAlive == 0 && currentWaveIndex < waves.Length)
        {
            StartCoroutine(StartNextWaveAfterDelay());
        }
    }

    private void StartWave()
    {
        if (currentWaveIndex >= waves.Length)
        {
            Debug.Log("All waves completed!");
            return;
        }
        
        // Calculate closest 3 spawn points to player
        activeSpawnPoints = GetClosestSpawnPoints(player.position, spawnPointsPerWave);
        
        // Set up wave
        Wave currentWave = waves[currentWaveIndex];
        enemiesLeftToSpawn = currentWave.enemyCount;
        
        Debug.Log($"Starting Wave {currentWaveIndex + 1} - Spawning {enemiesLeftToSpawn} enemies from {activeSpawnPoints.Length} closest spawn points");
        
        // Log which spawn points are being used
        for (int i = 0; i < activeSpawnPoints.Length; i++)
        {
            float distance = Vector3.Distance(player.position, activeSpawnPoints[i].position);
            Debug.Log($"Spawn Point {i + 1}: Distance = {distance:F2} units");
        }
        
        StartCoroutine(SpawnWave(currentWave));
    }

    private Transform[] GetClosestSpawnPoints(Vector3 targetPosition, int count)
    {
        if (allSpawnPoints == null || allSpawnPoints.Length == 0)
        {
            Debug.LogError("No spawn points assigned!");
            return new Transform[0];
        }
        
        // Use LINQ to sort spawn points by distance and take the closest ones
        Transform[] closestPoints = allSpawnPoints
            .Where(sp => sp != null) // Filter out any null references
            .OrderBy(sp => (sp.position - targetPosition).sqrMagnitude) // Sort by squared distance (more efficient)
            .Take(count) // Take only the number we need
            .ToArray();
        
        return closestPoints;
    }

    private IEnumerator SpawnWave(Wave wave)
    {
        isSpawning = true;
        
        while (enemiesLeftToSpawn > 0)
        {
            SpawnEnemy();
            enemiesLeftToSpawn--;
            yield return new WaitForSeconds(wave.timeBetweenSpawns);
        }
        
        isSpawning = false;
    }

    private void SpawnEnemy()
    {
        if (activeSpawnPoints.Length == 0) return;
        
        // Pick random spawn from the 3 closest points
        Transform spawnPoint = activeSpawnPoints[Random.Range(0, activeSpawnPoints.Length)];
        
        // Instantiate enemy
        GameObject newEnemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        enemiesAlive++;
        
        // Subscribe to enemy death
        EnemyAI enemyAI = newEnemy.GetComponent<EnemyAI>();
        if (enemyAI != null)
        {
            enemyAI.OnEnemyDeath += OnEnemyKilled;
        }
    }

    private void OnEnemyKilled()
    {
        enemiesAlive--;
        Debug.Log($"Enemy killed. Enemies remaining: {enemiesAlive}");
    }

    private IEnumerator StartNextWaveAfterDelay()
    {
        if (currentWaveIndex >= waves.Length - 1)
        {
            Debug.Log("All waves completed!");
            yield break;
        }
        
        float delay = waves[currentWaveIndex].timeToNextWave;
        Debug.Log($"Next wave in {delay} seconds...");
        
        yield return new WaitForSeconds(delay);
        
        currentWaveIndex++;
        StartWave(); // This will recalculate closest spawn points
    }

    // Optional: Visualize spawn points in editor
    private void OnDrawGizmos()
    {
        if (allSpawnPoints == null || player == null) return;
        
        // Draw all spawn points in yellow
        Gizmos.color = Color.yellow;
        foreach (Transform spawn in allSpawnPoints)
        {
            if (spawn != null)
            {
                Gizmos.DrawWireSphere(spawn.position, 1f);
            }
        }
        
        // Draw active spawn points in green during play mode
        if (Application.isPlaying && activeSpawnPoints != null)
        {
            Gizmos.color = Color.green;
            foreach (Transform spawn in activeSpawnPoints)
            {
                if (spawn != null)
                {
                    Gizmos.DrawWireSphere(spawn.position, 1.5f);
                    Gizmos.DrawLine(player.position, spawn.position);
                }
            }
        }
    }
}
