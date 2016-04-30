using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour {
    public float speed = 6.0F;
    public float jumpSpeed = 8.0F;
    public float gravity = 20.0F;
    private Vector3 moveDirection = Vector3.zero;

    private CharacterController CharController;
    public Transform camera;

    void Start()
    {
        CharController = GetComponent<CharacterController>();
    }

    bool IsInWater()
    {
        return transform.position.y < 0;
    }

    void Update() {
        if (CharController.isGrounded || IsInWater()) {
            moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            moveDirection = transform.TransformDirection(moveDirection);
            moveDirection *= speed;
            if (Input.GetKeyDown(KeyCode.X))
                moveDirection.y = jumpSpeed;
            
        }

        GameObject lookingAtPickupable = null;
        var rayHits = Physics.RaycastAll(new Ray(camera.transform.position, camera.transform.forward), 5);
        foreach (var hit in rayHits)
        {
            var pickupable = hit.transform.GetComponent<Pickupable>();
            if (pickupable != null)
            {
                lookingAtPickupable = pickupable.gameObject;
                break;
            }
        }

        moveDirection.y -= gravity * Time.deltaTime;
        if (IsInWater())
        {
            moveDirection *= 0.2f;
            moveDirection.y += gravity * 0.7f * Time.deltaTime;
        }
        CharController.Move(moveDirection * Time.deltaTime);

        var h = 2.0f * Input.GetAxis("Mouse X");
        transform.Rotate(0, h, 0);

        var v = -2.0f * Input.GetAxis("Mouse Y");
        camera.Rotate(v, 0, 0);
    }
}