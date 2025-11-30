using UnityEngine;

public class WeaponBobController : MonoBehaviour
{
    [Range(0.001f, 0.01f)]
    public float Amount = 0.002f;
    [Range(1f, 30f)]
    public float Frequency = 10.0f;
    [Range(10f, 100f)]
    public float Smooth = 10.0f;

    private Vector3 startPos;
    private bool isMoving;

    void Start()
    {
        startPos = transform.localPosition;
    }

    void Update()
    {
        if (isMoving)
        {
            ApplyWeaponBob();
        }
        else
        {
            ResetWeaponBob();
        }
    }

    private void ApplyWeaponBob()
    {
        Vector3 pos = Vector3.zero;
        // Vertical bobbing (Y axis)
        pos.y = Mathf.Sin(Time.time * Frequency) * Amount * 1.4f;
        // Horizontal sway (X axis) - slightly offset phase for realism
        pos.x = Mathf.Cos(Time.time * Frequency / 2f) * Amount * 1.6f;

        transform.localPosition = Vector3.Lerp(transform.localPosition, startPos + pos, Smooth * Time.deltaTime);
    }

    private void ResetWeaponBob()
    {
        transform.localPosition = Vector3.Lerp(transform.localPosition, startPos, Smooth * Time.deltaTime);
    }

    public void SetMoving(bool moving)
    {
        isMoving = moving;
    }
}
