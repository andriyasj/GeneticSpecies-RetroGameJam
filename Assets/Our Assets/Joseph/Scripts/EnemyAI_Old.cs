using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI_Old : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject bulletPrefab;

    [Header("Combat Settings")]
    [SerializeField] private float shootingRange = 10f;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private int damage = 10;

    [Header("Chase Settings")]
    [SerializeField] private float chaseSpeed = 5f;
    [SerializeField] private float destinationUpdateInterval = 0.5f; // How often to update chase destination

    [Header("Detection Settings")]
    [SerializeField] private float fieldOfView = 120f;
    [SerializeField] private LayerMask obstacleMask;

    private NavMeshAgent navAgent;
    private float nextFireTime = 0f;
    private float destinationUpdateTimer = 0f;
    private bool canShoot = false;

    public event Action OnEnemyDeath;

    private enum State
    {
        Chasing,
        Shooting
    }

    private State currentState = State.Chasing;

    void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();
        navAgent.speed = chaseSpeed;
        navAgent.stoppingDistance = shootingRange - 1f; // Stop slightly before shooting range

        // Find player automatically if not assigned
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        }

        // Start chasing immediately
        if (player != null)
        {
            navAgent.SetDestination(player.position);
        }
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        bool hasLineOfSight = CheckLineOfSight();

        // Check if within shooting range AND has line of sight
        if (distanceToPlayer <= shootingRange && hasLineOfSight)
        {
            currentState = State.Shooting;
        }
        else
        {
            currentState = State.Chasing;
        }

        // Execute state behavior
        switch (currentState)
        {
            case State.Chasing:
                ChasePlayer();
                if (animator != null)
                {
                    animator.SetBool("IsShooting", false);
                }
                break;

            case State.Shooting:
                ShootAtPlayer();
                if (animator != null)
                {
                    animator.SetBool("IsShooting", true);
                }
                break;
        }
    }

    private void ChasePlayer()
    {
        // Update destination periodically to follow moving player
        destinationUpdateTimer += Time.deltaTime;

        if (destinationUpdateTimer >= destinationUpdateInterval)
        {
            navAgent.SetDestination(player.position);
            destinationUpdateTimer = 0f;
        }

        // Make sure NavMeshAgent is active and moving
        if (!navAgent.isStopped)
        {
            navAgent.isStopped = false;
        }
    }

    private bool CheckLineOfSight()
    {
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Check field of view
        float angle = Vector3.Angle(transform.forward, directionToPlayer);
        if (angle > fieldOfView / 2f)
        {
            return false;
        }

        // Raycast to check for obstacles
        RaycastHit hit;
        Vector3 startPosition = transform.position + Vector3.up * 1f; // Raise raycast origin

        if (Physics.Raycast(startPosition, directionToPlayer, out hit, distanceToPlayer, ~0))
        {
            // Check if we hit the player (not an obstacle)
            if (hit.transform == player || hit.transform.CompareTag("Player"))
            {
                return true;
            }
        }

        return false;
    }

    private void ShootAtPlayer()
    {
        // Stop moving completely
        navAgent.isStopped = true;
        navAgent.velocity = Vector3.zero;

        // Rotate to face player smoothly
        Vector3 direction = (player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 8f);

        // Shoot at intervals
        if (Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + 1f / fireRate;
        }
    }

    private void Shoot()
    {
        // Spawn bullet projectile
        if (bulletPrefab != null && firePoint != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

            // Optional: Add velocity to bullet if it has rigidbody
            //Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
            //if (bulletRb != null)
            //{
            //    Vector3 shootDirection = (player.position - firePoint.position).normalized;
            //    bulletRb.linearVelocity = shootDirection * 20f; // Adjust bullet speed
            //}
        }
        else
        {
            RaycastHit hit;
            Vector3 shootDirection = (player.position - transform.position).normalized;

            if (Physics.Raycast(transform.position + Vector3.up, shootDirection, out hit, shootingRange))
            {
                Debug.Log($"Enemy shot at {hit.collider.name}");

                // Apply damage if hit player
                if (hit.transform.CompareTag("Player"))
                {
                    // Add your player damage logic here
                    // hit.transform.GetComponent<PlayerHealth>()?.TakeDamage(damage);
                }
            }
        }

        Debug.DrawRay(transform.position + Vector3.up, (player.position - transform.position).normalized * shootingRange, Color.red, 0.5f);
    }

    public void TakeDamage(int damageAmount)
    {
        // Handle damage and death
        OnEnemyDeath?.Invoke();
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        // Draw shooting range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, shootingRange);

        // Draw field of view
        if (player != null)
        {
            Vector3 forward = transform.forward * shootingRange;
            Vector3 leftBoundary = Quaternion.Euler(0, -fieldOfView / 2f, 0) * forward;
            Vector3 rightBoundary = Quaternion.Euler(0, fieldOfView / 2f, 0) * forward;

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
            Gizmos.DrawLine(transform.position, transform.position + rightBoundary);
        }
    }
}
