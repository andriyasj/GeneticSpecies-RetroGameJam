using UnityEngine;

public class StatManager : MonoBehaviour
{
    public static StatManager instance;

    [Header("Settings")]
    public float cameraSensitivity; // Slider 0.1 - 1

    [Header("Player Stats")]
    private int health = 100;
    public int Health
    {
        get => health;
        set
        {
            UIManager.Instance.SetHealth(health/100); //health is currently a 0-1 value not clamped
        }
    }
    [SerializeField] private int ammo = 10; 

    public int Ammo
    {
        get => ammo;
        set
        {
            ammo = value;
            UIManager.Instance.SetAmmo(ammo);
        }
    }
    public int speed;
    public float interactRange;
    public bool hasKey;
    public enum WeaponType
    {
        Enemy1,
        Enemy2,
        Enemy3,
        Enemy4
    }

    // [Header("Utility Stats")]


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.SetAmmo(ammo);
            UIManager.Instance.SetHealth(health / 100f); 
        }
    }
    
}
