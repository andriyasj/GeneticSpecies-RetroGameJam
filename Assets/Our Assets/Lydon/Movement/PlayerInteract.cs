using System;
using UnityEngine;

public interface ITakeover
{
    enum enemyType
    {
        Enemy1,
        Enemy2,
        Enemy3,
        Enemy4
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
                
                // TODO WRITE SWITCH STATEMENT BASED ON TYPE OF TAKEOVER
                
                Destroy(hitInfo.collider.gameObject);
            }
        }
    }
}