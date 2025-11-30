using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    public Camera cam;
    private float xRotation = 0f;
    
    public void ProcessLook(Vector2 input)
    {
        float mouseX = input.x;
        cam.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);        
        transform.Rotate(Vector3.up * (mouseX * StatManager.instance.cameraSensitivity));

    }
}
