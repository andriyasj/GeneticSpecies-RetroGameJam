using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerActions : MonoBehaviour
{
    void KillPlayer()
    {
        SceneManager.LoadScene(0);
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
