using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour {
    public float speed = 6.0F;
    public float jumpSpeed = 8.0F;
    public float gravity = 20.0F;
    private Vector3 moveDirection = Vector3.zero;

    private CharacterController CharController;
    public Transform camera;
    public GameObject hand;
    private GameObject lookingAtPickupable;
    public Texture2D ScreenFade;
    private float screenFadeCount = 0;

    void Start()
    {
        CharController = GetComponent<CharacterController>();

        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;
    }

    bool IsInWater()
    {
        return transform.position.y < 0;
    }

    void ToggleHand()
    {
        //var currentlyHolding = hand.transform.ch
        if (hand.transform.childCount > 0)
        {
            var children = new List<Transform>(hand.transform.childCount);
            for (var i = 0; i < hand.transform.childCount; i++)
            {
                children.Add(hand.transform.GetChild(i));
            }

            foreach (var child in children)
            {
                child.transform.parent = transform.parent;
                var rigidBody = child.GetComponent<Rigidbody>();
                if (rigidBody != null)
                {
                    rigidBody.isKinematic = false;
                }

                var pickupable = child.GetComponent<Pickupable>();
                if (pickupable != null)
                {
                    pickupable.CurrentlyBeingHeld = false;
                }
            }
        }
        else if (lookingAtPickupable != null)
        {
            lookingAtPickupable.transform.parent = hand.transform;
            lookingAtPickupable.transform.localPosition = Vector3.zero;
            lookingAtPickupable.transform.localRotation = Quaternion.identity;

            var rigidBody = lookingAtPickupable.GetComponent<Rigidbody>();
            if (rigidBody != null)
            {
                rigidBody.isKinematic = true;
            }

            var pickupable = lookingAtPickupable.GetComponent<Pickupable>();
            if (pickupable != null)
            {
                pickupable.CurrentlyBeingHeld = true;
            }
        }
    }

    void OnGUI()
    {
        if (screenFadeCount > 3)
        {
            return;
        }
        var alpha = 1f;
        if (screenFadeCount > 1)
        {
            alpha = 1f - ((screenFadeCount - 1.0f) * 0.5f);
        }

        GUI.color = new Color(1, 1, 1, alpha);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), ScreenFade);
    }

    void Update() {
        screenFadeCount += Time.deltaTime;
        if (CharController.isGrounded || IsInWater()) {
            moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            moveDirection = transform.TransformDirection(moveDirection);
            moveDirection *= speed;
            if (Input.GetKeyDown(KeyCode.X))
                moveDirection.y = jumpSpeed;
            
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            ToggleHand();
        }

        /*
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Cursor.visible)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        */

        lookingAtPickupable = null;
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        var rayHits = Physics.RaycastAll(ray, 5);
        foreach (var hit in rayHits)
        {
            var pickupable = hit.transform.GetComponent<Pickupable>();
            if (pickupable != null && !pickupable.CurrentlyBeingHeld)
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

        {
            var h = 2.0f * Input.GetAxis("Mouse X");
            transform.Rotate(0, h, 0);

            var v = -2.0f * Input.GetAxis("Mouse Y");
            camera.Rotate(v, 0, 0);
        }
    }
}