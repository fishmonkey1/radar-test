using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class tankSteer : MonoBehaviour
{
    [SerializeField] private CharacterController controller;
    [SerializeField] private Camera overHeadCam;
    [SerializeField] private Camera firstPersonCam;
    [SerializeField] private Canvas radarCanvas;


    [Header("Tank Settings")]
    [SerializeField] float maxSpeed = 50.0f;
    [SerializeField] float maxReverseSpeed = -2.0f;
    [SerializeField] float turnAngleMin = 20f;
    [SerializeField] float turnAngleMax = 50f;
    [SerializeField] float acceleration = 2.0f;
    [SerializeField] float decceleration = 5f;
    [SerializeField] [Range(0.01f, 0.3f)] float quatLerp = .07f;

    [Header("Current Values")]
    [SerializeField] float currSpeed;
    [SerializeField] float turnAngle;
    [SerializeField] float currPitch;
    [SerializeField] float engineForce;




    //[SerializeField] private GameManager gm;

    private Vector2 driverInput;
    private Vector3 playerVelocity;
    private bool groundedPlayer;
    private float gravityValue = -9.81f;

    LayerMask layerMask;
    // FYI can also pass multiple values like this:
    //int layerMmask = LayerMask.GetMask("Terrain", "Enemy"); 

    private void Start()
    {
        layerMask = LayerMask.GetMask("Terrain");
        firstPersonCam.enabled = true;
        overHeadCam.enabled = false;
    }

    private void FixedUpdate()
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
        transform.Rotate(Vector3.up, turnAngle * driverInput.x * turnAngle * Time.deltaTime);
        controller.Move(transform.forward * currSpeed * Time.deltaTime);



        RaycastHit groundHit;
        if (Physics.Raycast(transform.position + new Vector3(0f, 1, 0f), transform.TransformDirection(Vector3.down), out groundHit, 10f, layerMask))
        {
            if (groundHit.normal != transform.up)
            {
                Quaternion hitNormal = Quaternion.FromToRotation(transform.up, groundHit.normal) * transform.rotation;
                transform.rotation = Quaternion.Lerp(transform.rotation, hitNormal, quatLerp);
            }

        }



        if (driverInput.y != 0f)
        {
            currSpeed = Mathf.MoveTowards(currSpeed, maxSpeed * driverInput.y, acceleration * Time.deltaTime);
        }
        else
        {
            //if not accel, move speed towards 0
            currSpeed = Mathf.MoveTowards(currSpeed, 0, decceleration * Time.deltaTime);
        }
        Mathf.Clamp(currSpeed, maxReverseSpeed, maxSpeed);

        float turnAngleMaffs = turnAngleMax * (1f - (Mathf.InverseLerp(0, maxSpeed, Mathf.Abs(currSpeed))));
        turnAngle = Mathf.Clamp(turnAngleMaffs, turnAngleMin, turnAngleMax);


    }

    public void OnMove(InputValue value)
    {
        driverInput = value.Get<Vector2>();
    }

    public void OnCameraToggle()
    {
        if (firstPersonCam.enabled)
        {
            firstPersonCam.enabled = false;
            overHeadCam.enabled = true;
        }
        else
        {
            firstPersonCam.enabled = true;
            overHeadCam.enabled = false;
        }
    }

    public void OnToggleRadarMinimap()
    {
        if (radarCanvas.enabled)
        {
            radarCanvas.enabled = false;
        }
        else
        {
            radarCanvas.enabled = true;
        }
    }

}
