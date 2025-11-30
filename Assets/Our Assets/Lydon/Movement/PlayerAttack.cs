using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    private float nextFireTime = 0f;

    [Header("Shooting Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 0.3f;

    public void Attack()
    {
        if (StatManager.instance.ammo > 0 && Time.time >= nextFireTime)
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

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        StatManager.instance.Ammo--;
<<<<<<< Updated upstream
=======

        if (usingPlasma)
        {
            Ray ray = new Ray(firePoint.transform.position, firePoint.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hitInfo, StatManager.instance.interactRange))
            {
                if (hitInfo.transform.gameObject.CompareTag("Enemy"))
                {
                    EnemyAI enemy = hitInfo.transform.GetComponent<EnemyAI>();
                    enemy.TakeDamage(10);
                }
            }
        }
>>>>>>> Stashed changes
    }
}
