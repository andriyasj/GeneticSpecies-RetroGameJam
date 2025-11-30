using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    private float nextFireTime = 0f;

    [Header("Bullet FX")]
    [SerializeField] private GameObject bulletFx;
    [SerializeField] private GameObject laserFx;
    [SerializeField] private GameObject fireballFx;
    [Header("Guns")]
    [SerializeField] private GameObject pistol;
    [SerializeField] private GameObject plasmaGun;
    [SerializeField] private GameObject rocket;
    [Header("Settings")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 0.3f;
    


    private GameObject bulletPrefab;
    private bool usingPlasma;

    private void Start()
    {
        bulletPrefab = bulletFx;
        usingPlasma = false;
    }

    public void Attack()
    {
        if (StatManager.instance.ammo > 0 && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    public void ChangeWeapon(int weaponId)
    {
        switch (weaponId)
        {
            case 1:
                Debug.Log("Switched to Plasma.");

                pistol.SetActive(false);
                plasmaGun.SetActive(true);
                rocket.SetActive(false);
                AudioManager.Instance?.PlaySwappingSFX();
                usingPlasma = true;
                bulletPrefab = laserFx;
                break;
            case 2:
                Debug.Log("Switched to Rocket.");

                pistol.SetActive(false);
                plasmaGun.SetActive(false);
                rocket.SetActive(true);
                AudioManager.Instance?.PlaySwappingSFX();
                usingPlasma = false;
                bulletPrefab = fireballFx;
                break;
            default:
                Debug.LogWarning("Unknown weapon ID!");
                break;
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
        
        AudioManager.Instance?.PlayGunShoot();


        if (usingPlasma)
        {
            AudioManager.Instance?.PlayLaserShoot();

            Ray ray = new Ray(firePoint.transform.position, firePoint.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hitInfo, StatManager.instance.interactRange))
            {
                if (hitInfo.transform.gameObject.tag == "Enemy")
                {
                    EnemyAI enemy = hitInfo.transform.GetComponent<EnemyAI>();
                    enemy.TakeDamage(10);
                }
            }
        }
        
        
    }
}
