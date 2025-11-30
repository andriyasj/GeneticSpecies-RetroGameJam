using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Shooting Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float bulletSpeed = 20f;
    [SerializeField] private float fireRate = 0.3f; // Time between shots

    [Header("Aiming Settings")]
    [SerializeField] private float rotationSpeed = 10f; // How fast player rotates to face aim direction

    private CharacterController controller;
    private Vector3 velocity;
    private float nextFireTime = 0f;
    private float currentYRotation = 0f; // Track horizontal rotation angle

    [Header("SFX Clips")]
    [SerializeField] private AudioClip playerShoot;
    void Start()
    {
        controller = GetComponent<CharacterController>();

        // Lock cursor for better gameplay experience (optional)
        // Cursor.lockState = CursorLockMode.Locked;
        // Cursor.visible = false;
    }

    void Update()
    {
        HandleMovement();
        HandleAiming();
        HandleShooting();
    }

    private void HandleMovement()
    {
        // Get WASD input
        float horizontal = Input.GetAxis("Horizontal"); // A/D or Left/Right arrows
        float vertical = Input.GetAxis("Vertical");     // W/S or Up/Down arrows

        // Create movement direction (relative to world, not player rotation)
        Vector3 moveDirection = new Vector3(horizontal, 0f, vertical).normalized;

        // Move the player
        if (moveDirection.magnitude >= 0.1f)
        {
            controller.Move(moveDirection * moveSpeed * Time.deltaTime);
        }

        // Apply gravity
        if (controller.isGrounded)
        {
            velocity.y = -2f; // Keep player grounded
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }

        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleAiming()
    {
        // Get horizontal mouse movement (only X-axis)
        float mouseX = Input.GetAxis("Mouse X");

        // Update rotation based on mouse horizontal movement
        currentYRotation += mouseX * rotationSpeed;

        // Apply rotation ONLY on Y-axis (horizontal rotation)
        transform.rotation = Quaternion.Euler(0f, currentYRotation, 0f);
    }

    private void HandleShooting()
    {
        // Shoot on left mouse button or spacebar
        if ((Input.GetButton("Fire1") || Input.GetKey(KeyCode.Space)) && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    private void Shoot()
    {
        if (bulletPrefab == null || firePoint == null)
        {
            Debug.LogWarning("Bullet prefab or fire point not assigned!");
            return;
        }
        
        AudioManager.Instance?.PlayGunShoot();

        // Instantiate bullet at fire point
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        // Get the forward direction (only horizontal, no vertical component)
        Vector3 shootDirection = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;

        // Apply velocity to bullet
        Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
        if (bulletRb != null)
        {
            bulletRb.linearVelocity = shootDirection * bulletSpeed;
        }

        // Destroy bullet after 5 seconds to prevent buildup
        Destroy(bullet, 5f);
    }

    

    // Optional: Draw aiming direction in Scene view
    private void OnDrawGizmos()
    {
        if (firePoint != null)
        {
            Gizmos.color = Color.red;
            Vector3 aimDirection = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
            Gizmos.DrawRay(firePoint.position, aimDirection * 5f);
        }
    }
}
