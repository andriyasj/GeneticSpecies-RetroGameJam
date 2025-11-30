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
    public Camera playerCamera;
    //private bool _canInteract = false;

    public void AttemptTakeover()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, StatManager.instance.interactRange))
        {
            if (hitInfo.collider.gameObject.TryGetComponent(out ITakeover takeoverObj))
            {
                ITakeover.enemyType type = takeoverObj.Takeover();
                
                switch (type)
                {
                    case ITakeover.enemyType.Enemy1:
                        Debug.Log("Took over Enemy 1! Applying stats...");
                        StatManager.instance.health = 40;
                        break;
                    case ITakeover.enemyType.Enemy2:
                        Debug.Log("Took over Enemy 2! Applying stats...");
                        StatManager.instance.health = 40;
                        break;
                    default:
                        Debug.LogWarning("Unknown enemy type taken over.");
                        break;
                }
            }
        }
    }
}