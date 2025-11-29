using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private int damage;

    void Start()
    {
        StartCoroutine(DestroySelf());
    }

    void OnParticleCollision(GameObject other)
    {
        if (other.name == "Player")
        {
            //Player player = other.GetComponent<Player>();
            //player.TakeDamage(damage);
        }
    }

    private IEnumerator DestroySelf()
    {
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }
}
