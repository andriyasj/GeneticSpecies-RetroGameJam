using System;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerActions : MonoBehaviour
{
    void KillPlayer()
    {
        Destroy(gameObject);
    }
    public void GetAmmo(int ammo)
    {
        StatManager.instance.Ammo = ammo;
    }
    public void TakeDamage(int damage)
    {
        StatManager.instance.Health -= damage;
        if (StatManager.instance.Health <= 0) KillPlayer();
    }
    public void HealPlayer(int healAmount)
    {
        StatManager.instance.Health += healAmount;
    }

    public void PickupKey(bool hasKey)
    {
        StatManager.instance.hasKey = hasKey;
    }
}
