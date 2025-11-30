using System.Numerics;
using Unity.Hierarchy;
using UnityEngine;
using UnityEngine.EventSystems;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class PlayerMotors : MonoBehaviour
{
    private CharacterController controller;
    private HeadBobController headBobController;
    private WeaponBobController weaponBobController;
    private Vector3 playerVelocity;
    private bool isGrounded;
    public float gravity = -9.8f;
    private Vector2 lastInput;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        headBobController = GetComponentInChildren<HeadBobController>();
        weaponBobController = GetComponentInChildren<WeaponBobController>();
    }
    
    void Update()
    {
        isGrounded = controller.isGrounded;
    }

    public void ProcessMove(Vector2 input)
    {
        Vector3 moveDirection = Vector3.zero;
        moveDirection.x = input.x;
        moveDirection.z = input.y;
        controller.Move(transform.TransformDirection(moveDirection) * (StatManager.instance.speed * Time.deltaTime));
        bool isMoving = input.magnitude > 0 && isGrounded;
        if (headBobController != null)
            headBobController.SetMoving(isMoving);
        if (weaponBobController != null)
            weaponBobController.SetMoving(isMoving);
        if (isGrounded && playerVelocity.y < 0)
            playerVelocity.y = -2f;
        playerVelocity.y += gravity * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
        lastInput = input;
    }

}
