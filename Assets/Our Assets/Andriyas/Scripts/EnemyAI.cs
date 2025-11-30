using System;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour, ITakeover
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private GameObject explosionPrefab;

    [Header("Enemy Type")]
    [SerializeField] private ITakeover.enemyType enemyType;
    
    [Header("Combat Settings")]
    [SerializeField] private float shootingRange = 10f;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private int maxHealth = 50;
    [SerializeField] private int damage = 10;

    [Header("Chase Settings")]
    [SerializeField] private float chaseSpeed = 5f;
    [SerializeField] private float destinationUpdateInterval = 0.5f;

    [Header("Detection Settings")]
    [SerializeField] private float detectionRadius = 30f;
    [SerializeField] private float fieldOfView = 120f;
    [SerializeField] private LayerMask obstacleMask;

    private NavMeshAgent navAgent;
    private float nextFireTime;
    private float destinationUpdateTimer;
    private int currentHealth;
    private bool isAlive = true;
    private bool inShootingState;
    private int currentDamage;

    // Cached vectors to reduce allocations
    private Vector3 cachedDirection;
    private Vector3 raycastOrigin;

    public event Action OnEnemyDeath;

    private enum State
    {
        Idle,
        Chasing,
        Shooting
    }

    private State currentState = State.Idle;
    private State previousState = State.Idle;

    void Awake()
    {
        navAgent = GetComponent<NavMeshAgent>();
        currentHealth = maxHealth;
    }

    void Start()
    {
        navAgent.speed = chaseSpeed;
        navAgent.stoppingDistance = shootingRange - 1f;

        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        }

        if (player != null)
        {
            navAgent.SetDestination(player.position);
        }

        raycastOrigin = Vector3.up;
    }

    public ITakeover.enemyType Takeover()
    {
        if (player != null)
        {
            Vector3 oldPlayerPos = player.position;

            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            player.position = transform.position;
            player.rotation = transform.rotation;

            if (cc != null) cc.enabled = true;

            if (explosionPrefab != null)
            {
                Instantiate(explosionPrefab, oldPlayerPos, Quaternion.identity);
            }
        }
        Die();
        return enemyType;
    }
    
    void Update()
    {
        if (!isAlive || player == null) return;

        EvaluateState();
        ExecuteState();
    }

    private void EvaluateState()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // Early exit if player is out of detection radius
        if (distanceToPlayer > detectionRadius)
        {
            currentState = State.Idle;
            return;
        }

        bool hasLineOfSight = IsPlayerInLineOfSight(distanceToPlayer);
        bool inRange = distanceToPlayer <= shootingRange;

        currentState = (inRange && hasLineOfSight) ? State.Shooting : State.Chasing;
    }

    private void ExecuteState()
    {
        if (currentState != previousState)
        {
            OnStateChanged();
            previousState = currentState;
        }

        switch (currentState)
        {
            case State.Idle:
                IdleState();
                break;
            case State.Chasing:
                ChasingState();
                break;
            case State.Shooting:
                ShootingState();
                break;
        }
    }

    private void OnStateChanged()
    {
        if (animator == null) return;

        switch (currentState)
        {
            case State.Shooting:
                animator.SetBool("IsShooting", true);
                navAgent.isStopped = true;
                navAgent.velocity = Vector3.zero;
                inShootingState = true;
                break;
            case State.Chasing:
            case State.Idle:
                animator.SetBool("IsShooting", false);
                navAgent.isStopped = false;
                inShootingState = false;
                break;
        }
    }

    private void IdleState()
    {
        navAgent.isStopped = true;
    }

    private void ChasingState()
    {
        destinationUpdateTimer += Time.deltaTime;

        if (destinationUpdateTimer >= destinationUpdateInterval)
        {
            navAgent.SetDestination(player.position);
            destinationUpdateTimer = 0f;
        }
    }

    private void ShootingState()
    {
        // Rotate to face player
        cachedDirection = (player.position - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(new Vector3(cachedDirection.x, 0, cachedDirection.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 8f);

        // Shoot at intervals
        if (Time.time >= nextFireTime)
        {
            Fire();
            nextFireTime = Time.time + (1f / fireRate);
        }
    }

    private bool IsPlayerInLineOfSight(float distanceToPlayer)
    {
        cachedDirection = (player.position - transform.position).normalized;

        // Check field of view
        float angleToPlayer = Vector3.Angle(transform.forward, cachedDirection);
        if (angleToPlayer > fieldOfView * 0.5f)
        {
            return false;
        }

        // Raycast check
        Physics.Raycast(transform.position + raycastOrigin, cachedDirection, out RaycastHit hit, distanceToPlayer, ~obstacleMask);
        return hit.transform == player || (hit.collider != null && hit.transform.CompareTag("Player"));
    }

    private void Fire()
    {
        if (bulletPrefab == null || firePoint == null) return;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        
        switch (enemyType)
        {
            case ITakeover.enemyType.Enemy1:
                Ray ray = new Ray(firePoint.transform.position, firePoint.transform.forward);
                Debug.DrawRay(ray.origin, ray.direction * 5f, Color.blue);
                if (Physics.Raycast(ray, out RaycastHit hitInfo, StatManager.instance.interactRange))
                {
                    if (hitInfo.transform.CompareTag("Player"))
                    {
                        print("Player Hit");
                        PlayerActions player = hitInfo.transform.gameObject.GetComponent<PlayerActions>();
                        player.TakeDamage(damage);
                    }
                }
                break;

            case ITakeover.enemyType.Enemy2:
        
                Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
                if (bulletRb != null)
                {
                    cachedDirection = (player.position - firePoint.position).normalized;
                    bulletRb.linearVelocity = cachedDirection * 20f;
                }
                break;

            default:
                break;
        }
    }

    public void TakeDamage(int damageAmount)
    {
        if (!isAlive) return;

        currentHealth -= damageAmount;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isAlive = false;
        navAgent.isStopped = true;
        navAgent.enabled = false;

        if (animator != null)
        {
            animator.SetTrigger("Death");
        }

        OnEnemyDeath?.Invoke();

        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        // Destroy after a short delay to allow death animation
        Destroy(gameObject, 0.5f);
    }

    private void OnDrawGizmosSelected()
    {
        // Detection radius
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // Shooting range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, shootingRange);

        // Field of view
        if (player != null)
        {
            Gizmos.color = Color.yellow;
            Vector3 forward = transform.forward * shootingRange;
            Vector3 left = Quaternion.Euler(0, -fieldOfView * 0.5f, 0) * forward;
            Vector3 right = Quaternion.Euler(0, fieldOfView * 0.5f, 0) * forward;

            Gizmos.DrawLine(transform.position, transform.position + left);
            Gizmos.DrawLine(transform.position, transform.position + right);
            Gizmos.DrawLine(transform.position + left, transform.position + right);
        }
    }
    
    public void ScaleHealth(float multiplier)
    {
        currentHealth = Mathf.RoundToInt(maxHealth * multiplier);
        Debug.Log($"Enemy health scaled to: {currentHealth} (x{multiplier:F2})");
    }

    public void ScaleDamage(float multiplier)
    {
        currentDamage = Mathf.RoundToInt(damage * multiplier);
        Debug.Log($"Enemy damage scaled to: {currentDamage} (x{multiplier:F2})");
    }
}
