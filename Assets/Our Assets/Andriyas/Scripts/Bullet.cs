using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private int damage;
    
    public GameObject owner;

    void Start()
    {
        StartCoroutine(DestroySelf());
    }

    void OnParticleCollision(GameObject other)
    {
        if (other.CompareTag("Enemy"))
        {
            print("Enemy Hit");
            EnemyAI enemy = other.GetComponent<EnemyAI>();
            enemy.TakeDamage(damage);
        }
        if (other.CompareTag("Player"))
        {
            print("Player Hit");
            PlayerActions player = other.GetComponent<PlayerActions>();
            player.TakeDamage(damage);
        }
    }

    private IEnumerator DestroySelf()
    {
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }
}
