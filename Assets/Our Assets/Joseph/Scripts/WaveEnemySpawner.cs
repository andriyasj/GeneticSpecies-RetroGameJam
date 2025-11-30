using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WaveEnemySpawner : MonoBehaviour
{
    [Header("Enemy Settings")]
    [SerializeField] private GameObject enemyPrefab;
    
    [Header("Spawn Points")]
    [SerializeField] private Transform[] allSpawnPoints;
    [SerializeField] private int spawnPointsPerWave = 3;
    
    [Header("Player Reference")]
    [SerializeField] private Transform player;
    
    [Header("Initial Wave Settings")]
    [SerializeField] private int initialEnemyCount = 5; // First wave enemy count
    [SerializeField] private float initialTimeBetweenSpawns = 1f;
    [SerializeField] private float initialTimeToNextWave = 10f;
    
    [Header("Base Wave Settings (After Wave 1)")]
    [SerializeField] private int baseEnemyCount = 10; // Starting number for wave 2+
    [SerializeField] private float baseTimeBetweenSpawns = 0.8f;
    [SerializeField] private float baseTimeToNextWave = 8f;
    
    [Header("Difficulty Scaling")]
    [SerializeField] private float enemyCountMultiplier = 1.2f;
    [SerializeField] private int enemyCountAdditive = 5;
    [SerializeField] private float spawnRateIncrease = 1f;
    [SerializeField] private float minTimeBetweenSpawns = 0.2f;
    [SerializeField] private float waveDelayDecrease = 0.95f;
    [SerializeField] private float minTimeToNextWave = 3f;
    
    [Header("Enemy Health/Damage Scaling")]
    [SerializeField] private bool scaleEnemyStats = true;
    [SerializeField] private float healthScalingPerWave = 1.15f;
    [SerializeField] private float damageScalingPerWave = 1.1f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    private int currentWaveNumber = 1;
    private int enemiesLeftToSpawn = 0;
    private int enemiesAlive = 0;
    private Transform[] activeSpawnPoints;
    private bool isSpawning = false;
    private bool waitingForNextWave = false;
    
    private int currentEnemyCount;
    private float currentTimeBetweenSpawns;
    private float currentTimeToNextWave;

    void Start()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        }
        
        // Start the first wave immediately
        StartWave();
    }

    void Update()
    {
        // Check if wave is complete and not already waiting
        if (!isSpawning && enemiesAlive == 0 && !waitingForNextWave)
        {
            waitingForNextWave = true;
            StartCoroutine(StartNextWaveAfterDelay());
        }
    }

    private void StartWave()
    {
        // Calculate wave difficulty
        CalculateWaveDifficulty();
        UIManager.Instance.SetEnemyCount(currentEnemyCount);
        UIManager.Instance.SetWaveCount(currentWaveNumber);
        // Calculate closest spawn points to player
        activeSpawnPoints = GetClosestSpawnPoints(player.position, spawnPointsPerWave);
        
        enemiesLeftToSpawn = currentEnemyCount;
        
        if (showDebugLogs)
        {
            Debug.Log($"<color=cyan>=== WAVE {currentWaveNumber} STARTING ===</color>");
            Debug.Log($"Spawning {currentEnemyCount} enemies at {currentTimeBetweenSpawns:F2}s intervals");
            Debug.Log($"Using {activeSpawnPoints.Length} closest spawn points to player");
            
            if (scaleEnemyStats && currentWaveNumber > 1)
            {
                float healthMult = Mathf.Pow(healthScalingPerWave, currentWaveNumber - 1);
                float damageMult = Mathf.Pow(damageScalingPerWave, currentWaveNumber - 1);
                Debug.Log($"Enemy Stats Multiplier - Health: x{healthMult:F2} | Damage: x{damageMult:F2}");
            }
        }
        
        StartCoroutine(SpawnWave());
    }

    private void CalculateWaveDifficulty()
    {
        // Special handling for Wave 1 (initial wave)
        if (currentWaveNumber == 1)
        {
            currentEnemyCount = initialEnemyCount;
            currentTimeBetweenSpawns = initialTimeBetweenSpawns;
            currentTimeToNextWave = initialTimeToNextWave;
            return;
        }
        
        // For waves 2+, use progressive scaling
        int waveOffset = currentWaveNumber - 2; // Wave 2 = 0, Wave 3 = 1, etc.
        
        currentEnemyCount = Mathf.RoundToInt(
            baseEnemyCount * Mathf.Pow(enemyCountMultiplier, waveOffset) + 
            (enemyCountAdditive * waveOffset)
        );
        
        currentTimeBetweenSpawns = Mathf.Max(
            baseTimeBetweenSpawns * Mathf.Pow(spawnRateIncrease, waveOffset),
            minTimeBetweenSpawns
        );
        
        currentTimeToNextWave = Mathf.Max(
            baseTimeToNextWave * Mathf.Pow(waveDelayDecrease, waveOffset),
            minTimeToNextWave
        );
    }

    private Transform[] GetClosestSpawnPoints(Vector3 targetPosition, int count)
    {
        if (allSpawnPoints == null || allSpawnPoints.Length == 0)
        {
            Debug.LogError("No spawn points assigned!");
            return new Transform[0];
        }
        
        Transform[] closestPoints = allSpawnPoints
            .Where(sp => sp != null)
            .OrderBy(sp => (sp.position - targetPosition).sqrMagnitude)
            .Take(count)
            .ToArray();
        
        return closestPoints;
    }

    private IEnumerator SpawnWave()
    {
        isSpawning = true;
        
        while (enemiesLeftToSpawn > 0)
        {
            SpawnEnemy();
            enemiesLeftToSpawn--;
            
            if (enemiesLeftToSpawn > 0) // Don't wait after last enemy
            {
                yield return new WaitForSeconds(currentTimeBetweenSpawns);
            }
        }
        
        isSpawning = false;
        
        if (showDebugLogs)
        {
            Debug.Log($"<color=yellow>Wave {currentWaveNumber} - All enemies spawned! Waiting for player to clear them...</color>");
        }
    }

    private void SpawnEnemy()
    {
        if (activeSpawnPoints.Length == 0) return;
        
        Transform spawnPoint = activeSpawnPoints[Random.Range(0, activeSpawnPoints.Length)];
        
        GameObject newEnemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        enemiesAlive++;
        
        // Scale enemy stats
        if (scaleEnemyStats)
        {
            ScaleEnemyStats(newEnemy);
        }
        
        // Subscribe to enemy death
        EnemyAI enemyAI = newEnemy.GetComponent<EnemyAI>();
        if (enemyAI != null)
        {
            enemyAI.OnEnemyDeath += OnEnemyKilled;
        }
    }

    private void ScaleEnemyStats(GameObject enemy)
    {
        EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
        if (enemyAI == null) return;
        
        float healthMultiplier = Mathf.Pow(healthScalingPerWave, currentWaveNumber - 1);
        float damageMultiplier = Mathf.Pow(damageScalingPerWave, currentWaveNumber - 1);
        
        enemyAI.ScaleHealth(healthMultiplier);
        enemyAI.ScaleDamage(damageMultiplier);
    }

    private void OnEnemyKilled()
    {
        enemiesAlive--;
        UIManager.Instance.SetEnemyCount(enemiesAlive);
        if (showDebugLogs)
        {
            Debug.Log($"<color=red>Enemy killed!</color> Remaining: {enemiesAlive}");
        }
        
        // Check if all enemies are dead
        if (enemiesAlive == 0 && !isSpawning)
        {
            if (showDebugLogs)
            {
                Debug.Log($"<color=green>WAVE {currentWaveNumber} COMPLETE!</color>");
            }
        }
    }

    private IEnumerator StartNextWaveAfterDelay()
    {
        if (showDebugLogs)
        {
            Debug.Log($"<color=cyan>Prepare for Wave {currentWaveNumber + 1} in {currentTimeToNextWave:F1} seconds...</color>");
        }
        
        yield return new WaitForSeconds(currentTimeToNextWave);
        
        currentWaveNumber++;
        UIManager.Instance.SetWaveCount(currentWaveNumber);
        waitingForNextWave = false;
        StartWave();
    }

    // Public methods for UI
    public int GetCurrentWave()
    {
        return currentWaveNumber;
    }

    public int GetEnemiesAlive()
    {
        return enemiesAlive;
    }

    public int GetEnemiesSpawned()
    {
        return currentEnemyCount - enemiesLeftToSpawn;
    }

    public bool IsWaveActive()
    {
        return isSpawning || enemiesAlive > 0;
    }

    private void OnDrawGizmos()
    {
        if (allSpawnPoints == null || player == null) return;
        
        Gizmos.color = Color.yellow;
        foreach (Transform spawn in allSpawnPoints)
        {
            if (spawn != null)
            {
                Gizmos.DrawWireSphere(spawn.position, 1f);
            }
        }
        
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
