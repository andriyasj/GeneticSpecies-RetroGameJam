using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WaveEnemySpawner : MonoBehaviour
{
    [Header("Enemy Settings")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject miniBossPrefab; // NEW: Miniboss prefab
    
    [Header("Spawn Points")]
    [SerializeField] private Transform[] allSpawnPoints;
    [SerializeField] private int spawnPointsPerWave = 3;
    
    [Header("Player Reference")]
    [SerializeField] private Transform player;
    
    [Header("Initial Wave Settings")]
    [SerializeField] private int initialEnemyCount = 5;
    [SerializeField] private float initialTimeBetweenSpawns = 1f;
    [SerializeField] private float initialTimeToNextWave = 10f;
    
    [Header("Base Wave Settings (After Wave 1)")]
    [SerializeField] private int baseEnemyCount = 10;
    [SerializeField] private float baseTimeBetweenSpawns = 0.8f;
    [SerializeField] private float baseTimeToNextWave = 8f;
    
    [Header("Miniboss Wave Settings")]
    [SerializeField] private int miniBossWaveInterval = 3; // Miniboss wave every 3 waves
    [SerializeField] private int miniBossCount = 3; // Number of minibosses to spawn
    [SerializeField] private float miniBossTimeBetweenSpawns = 2f;
    [SerializeField] private float miniBossWaveDelay = 15f; // Longer delay after miniboss wave
    
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
    [SerializeField] private float miniBossHealthMultiplier = 3f; // Minibosses have 3x health
    [SerializeField] private float miniBossDamageMultiplier = 1.5f; // Minibosses deal 1.5x damage
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    private int currentWaveNumber = 1;
    private int enemiesLeftToSpawn = 0;
    private int enemiesAlive = 0;
    private Transform[] activeSpawnPoints;
    private bool isSpawning = false;
    private bool waitingForNextWave = false;
    private bool isMiniBossWave = false; // NEW: Track if current wave is miniboss wave
    
    private int currentEnemyCount;
    private float currentTimeBetweenSpawns;
    private float currentTimeToNextWave;

    void Start()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        }
        
        StartWave();
    }

    void Update()
    {
        if (!isSpawning && enemiesAlive == 0 && !waitingForNextWave)
        {
            waitingForNextWave = true;
            StartCoroutine(StartNextWaveAfterDelay());
        }
    }

    private void StartWave()
    {
        // Check if this should be a miniboss wave
        isMiniBossWave = (currentWaveNumber % miniBossWaveInterval == 0);
        
        // Calculate wave difficulty
        CalculateWaveDifficulty();
        UIManager.Instance.SetEnemyCount(currentEnemyCount);
        UIManager.Instance.SetWaveCount(currentWaveNumber);
        
        // Calculate closest spawn points to player
        activeSpawnPoints = GetClosestSpawnPoints(player.position, spawnPointsPerWave);
        
        enemiesLeftToSpawn = currentEnemyCount;
        
        if (showDebugLogs)
        {
            if (isMiniBossWave)
            {
                Debug.Log($"<color=red>╔═══════════════════════════════════╗</color>");
                Debug.Log($"<color=red>║  MINIBOSS WAVE {currentWaveNumber} - {currentEnemyCount} MINIBOSSES! ║</color>");
                Debug.Log($"<color=red>╚═══════════════════════════════════╝</color>");
            }
            else
            {
                Debug.Log($"<color=cyan>=== WAVE {currentWaveNumber} STARTING ===</color>");
                Debug.Log($"Spawning {currentEnemyCount} enemies at {currentTimeBetweenSpawns:F2}s intervals");
            }
            
            Debug.Log($"Using {activeSpawnPoints.Length} closest spawn points to player");
            
            if (scaleEnemyStats && currentWaveNumber > 1)
            {
                float healthMult = Mathf.Pow(healthScalingPerWave, currentWaveNumber - 1);
                float damageMult = Mathf.Pow(damageScalingPerWave, currentWaveNumber - 1);
                
                if (isMiniBossWave)
                {
                    healthMult *= miniBossHealthMultiplier;
                    damageMult *= miniBossDamageMultiplier;
                    Debug.Log($"<color=orange>MINIBOSS Stats - Health: x{healthMult:F2} | Damage: x{damageMult:F2}</color>");
                }
                else
                {
                    Debug.Log($"Enemy Stats Multiplier - Health: x{healthMult:F2} | Damage: x{damageMult:F2}");
                }
            }
        }
        
        StartCoroutine(SpawnWave());
    }

    private void CalculateWaveDifficulty()
    {
        // MINIBOSS WAVE: Fixed count and timing
        if (isMiniBossWave)
        {
            currentEnemyCount = miniBossCount;
            currentTimeBetweenSpawns = miniBossTimeBetweenSpawns;
            currentTimeToNextWave = miniBossWaveDelay;
            return;
        }
        
        // REGULAR WAVE 1: Initial settings
        if (currentWaveNumber == 1)
        {
            currentEnemyCount = initialEnemyCount;
            currentTimeBetweenSpawns = initialTimeBetweenSpawns;
            currentTimeToNextWave = initialTimeToNextWave;
            return;
        }
        
        // REGULAR WAVES 2+: Progressive scaling
        // Calculate how many regular waves have passed (excluding miniboss waves)
        int regularWavesPassed = currentWaveNumber - (currentWaveNumber / miniBossWaveInterval) - 1;
        
        currentEnemyCount = Mathf.RoundToInt(
            baseEnemyCount * Mathf.Pow(enemyCountMultiplier, regularWavesPassed) + 
            (enemyCountAdditive * regularWavesPassed)
        );
        
        currentTimeBetweenSpawns = Mathf.Max(
            baseTimeBetweenSpawns * Mathf.Pow(spawnRateIncrease, regularWavesPassed),
            minTimeBetweenSpawns
        );
        
        currentTimeToNextWave = Mathf.Max(
            baseTimeToNextWave * Mathf.Pow(waveDelayDecrease, regularWavesPassed),
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
            
            if (enemiesLeftToSpawn > 0)
            {
                yield return new WaitForSeconds(currentTimeBetweenSpawns);
            }
        }
        
        isSpawning = false;
        
        if (showDebugLogs)
        {
            if (isMiniBossWave)
            {
                Debug.Log($"<color=orange>All {miniBossCount} Minibosses spawned! Defeat them to continue...</color>");
            }
            else
            {
                Debug.Log($"<color=yellow>Wave {currentWaveNumber} - All enemies spawned! Waiting for player to clear them...</color>");
            }
        }
    }

    private void SpawnEnemy()
    {
        if (activeSpawnPoints.Length == 0) return;
        
        Transform spawnPoint = activeSpawnPoints[Random.Range(0, activeSpawnPoints.Length)];
        
        // Choose which prefab to spawn based on wave type
        GameObject prefabToSpawn = isMiniBossWave ? miniBossPrefab : enemyPrefab;
        
        if (prefabToSpawn == null)
        {
            Debug.LogError(isMiniBossWave ? "MiniBoss prefab not assigned!" : "Enemy prefab not assigned!");
            return;
        }
        
        GameObject newEnemy = Instantiate(prefabToSpawn, spawnPoint.position, spawnPoint.rotation);
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
        
        // Apply extra multipliers for minibosses
        if (isMiniBossWave)
        {
            healthMultiplier *= miniBossHealthMultiplier;
            damageMultiplier *= miniBossDamageMultiplier;
        }
        
        enemyAI.ScaleHealth(healthMultiplier);
        enemyAI.ScaleDamage(damageMultiplier);
    }

    private void OnEnemyKilled()
    {
        enemiesAlive--;
        UIManager.Instance.SetEnemyCount(enemiesAlive);
        
        if (showDebugLogs)
        {
            if (isMiniBossWave)
            {
                Debug.Log($"<color=orange>Miniboss defeated!</color> Remaining: {enemiesAlive}");
            }
            else
            {
                Debug.Log($"<color=red>Enemy killed!</color> Remaining: {enemiesAlive}");
            }
        }
        
        if (enemiesAlive == 0 && !isSpawning)
        {
            if (showDebugLogs)
            {
                if (isMiniBossWave)
                {
                    Debug.Log($"<color=green>★★★ MINIBOSS WAVE {currentWaveNumber} COMPLETE! ★★★</color>");
                }
                else
                {
                    Debug.Log($"<color=green>WAVE {currentWaveNumber} COMPLETE!</color>");
                }
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

    public bool IsMiniBossWave()
    {
        return isMiniBossWave;
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
            // Change color for miniboss waves
            Gizmos.color = isMiniBossWave ? Color.red : Color.green;
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
