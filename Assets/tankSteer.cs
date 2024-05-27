using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class tankSteer : MonoBehaviour
{
    /*    [SerializeField] float maxSpeed = 50.0f;
        [SerializeField] float maxReverseSpeed = -2.0f;
        [SerializeField] float turnAngleMin = 20f;
        [SerializeField] float turnAngleMax = 50f;
        [SerializeField] float acceleration = 2.0f;
        [SerializeField] float decceleration = 5f;
        [SerializeField] float currSpeed;
        [SerializeField] float turnAngle;
        [SerializeField] float currPitch;

        [SerializeField] float engineForce;*/

    [SerializeField] private CharacterController controller;
    [SerializeField] float playerSpeed = 50.0f;
    [SerializeField] float playerRotationSpeed = 50f;

    [SerializeField] private GameManager gm;

    private Vector2 driverInput;
    private Vector3 playerVelocity;
    private bool groundedPlayer;
    private float gravityValue = -9.81f;

    [SerializeField] LayerMask layerMask;
    // FYI can also pass multiple values like this:
    //int layerMmask = LayerMask.GetMask("Terrain", "Enemy"); 

    private void Start()
    {
        layerMask = LayerMask.GetMask("Terrain");
    }

    private void Update()
    {
        groundedPlayer = controller.isGrounded;
        // stop player when hit ground
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }

        // gravity to keep on grounds
        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);

        // forward/back and rotate
        transform.Rotate(Vector3.up, playerRotationSpeed * driverInput.x * Time.deltaTime);
        controller.Move(transform.forward * driverInput.y * playerSpeed * Time.deltaTime);


        if (!groundedPlayer)
        {
            RaycastHit groundHit;
            if (Physics.Raycast(transform.position + new Vector3(0f, 1, 0f), transform.TransformDirection(Vector3.down), out groundHit, 10f, layerMask))
            {
                if (groundHit.normal != transform.up)
                {
                    transform.rotation = Quaternion.FromToRotation(transform.up, groundHit.normal) * transform.rotation;
                }

            }
        }
    }

    private void OnMove(InputValue value)
    {
        driverInput = value.Get<Vector2>();
    }

}
