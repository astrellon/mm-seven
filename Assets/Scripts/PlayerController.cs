using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour {
    public float speed = 6.0F;
    public float jumpSpeed = 8.0F;
    public float gravity = 20.0F;
    private Vector3 moveDirection = Vector3.zero;

    private CharacterController CharController;
    public Transform Camera;

    void Start()
    {
        CharController = GetComponent<CharacterController>();
    }

    void Update() {
        if (CharController.isGrounded) {
            moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            moveDirection = transform.TransformDirection(moveDirection);
            moveDirection *= speed;
            if (Input.GetKeyDown(KeyCode.X))
                moveDirection.y = jumpSpeed;
            
        }
        moveDirection.y -= gravity * Time.deltaTime;
        CharController.Move(moveDirection * Time.deltaTime);

        var h = 2.0f * Input.GetAxis("Mouse X");
        transform.Rotate(0, h, 0);

        var v = -2.0f * Input.GetAxis("Mouse Y");
        Camera.Rotate(v, 0, 0);
    }
}