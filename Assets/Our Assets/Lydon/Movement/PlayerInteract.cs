using System;
using UnityEngine;

public interface ITakeover
{
    enum enemyType
    {
        Enemy1,
        Enemy2
    }

    enemyType Takeover();
}

public class PlayerInteract : MonoBehaviour
{
    public PlayerAttack playerAttack;
    public Camera playerCamera;

    [Header("Takeover Cooldown")]
    public float takeoverCooldown = 5f; // seconds
    private bool canTakeover = true;

    private void Start()
    {
        playerAttack = GetComponent<PlayerAttack>();
    }

    public void AttemptTakeover()
    {
        if (!canTakeover)
        {
            Debug.Log("Takeover on cooldown!");
            return;
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, StatManager.instance.interactRange))
        {
            if (hitInfo.collider.gameObject.TryGetComponent(out ITakeover takeoverObj))
            {
                ITakeover.enemyType type = takeoverObj.Takeover();
                HandleTakeover(type);

                StartCoroutine(TakeoverCooldownRoutine());
            }
        }
    }

    private void HandleTakeover(ITakeover.enemyType type)
    {
        switch (type)
        {
            case ITakeover.enemyType.Enemy1:
                Debug.Log("Took over Enemy 1! Applying stats...");
                StatManager.instance.Health += 20;
                StatManager.instance.Ammo = 10;
                playerAttack.ChangeWeapon(1);
                break;

            case ITakeover.enemyType.Enemy2:
                Debug.Log("Took over Enemy 2! Applying stats...");
                StatManager.instance.Health += 40;
                StatManager.instance.Ammo = 5;
                playerAttack.ChangeWeapon(2);
                break;

            default:
                Debug.LogWarning("Unknown enemy type taken over.");
                break;
        }
    }

    private System.Collections.IEnumerator TakeoverCooldownRoutine()
    {
        canTakeover = false;
        UIManager.Instance.Indicator(false);
        Debug.Log("Takeover cooldown started...");

        yield return new WaitForSeconds(takeoverCooldown);

        canTakeover = true;
        UIManager.Instance.Indicator(true);
        Debug.Log("Takeover ready!");
    }
}