using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;

public class tankSteer : NetworkBehaviour, IRoleNeeded
{
    [SerializeField] private CharacterController controller;
    [SerializeField] private Canvas radarCanvas;
    private Camera currentCam;


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

    public Role RoleNeeded => CrewRoles.Driver;
    PlayerInfo playerInfo;


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
    }

    public void SetPlayer(PlayerInfo info)
    {
        Debug.Log("Assigning local player to tankSteer. info's role is " + info.CurrentRole);
        if(RoleNeeded.Name == info.CurrentRole.Name)
        {
            Debug.Log("Player's role matches for tankSteer");
            currentCam = CamCycle.Instance.GetFirstCamera(RoleNeeded);
        }
        playerInfo = info;
        if (playerInfo.OnRoleChange == null)
            playerInfo.OnRoleChange = new PlayerInfo.RoleChangeDelegate(OnRoleChange);
        else
            playerInfo.OnRoleChange += OnRoleChange;
        //We have to fetch a camera in Start since the debug stuff assumes you start as the driver
        //PlayerInfo does the PickRole stuff for the driver before this class registers for the delegate
        //So we can't just handle it normally in OnRoleChange for now until there's UI for picking roles
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

    public void OnRoleChange(Role oldRole, Role newRole)
    {
        if (newRole != RoleNeeded) return;

        //Otherwise we do any setup in here
        currentCam = CamCycle.Instance.GetFirstCamera(RoleNeeded); //Fetch the camera for the driver so it's active
        Debug.Log($"Got first camera for {RoleNeeded.Name} role");
    }

    public void OnMove(InputValue value)
    {
        if (playerInfo == null)
            return; //This role is unused, so do nothing
        if (!((IRoleNeeded)this).HaveRole(playerInfo.CurrentRole))
            return; //Don't allow driving inputs if you don't have the driver role selected
        if (isServer) //Only apply locally if you are the host
            driverInput = value.Get<Vector2>();
        else
            CmdSendInputs(value.Get<Vector2>()); //Otherwise send to the server
    }

    [Command(requiresAuthority = false)]
    public void CmdSendInputs(Vector2 input)
    {
        //Set the inputs on the server and then let it replicate the tank's position for everyone else
        driverInput.x = input.x;
        driverInput.y = input.y;
    }

    public void OnCameraToggle()
    {
        if (playerInfo == null)
            return; //This role is unused, so do nothing
        if (!((IRoleNeeded)this).HaveRole(playerInfo.CurrentRole))
            return;

        currentCam = CamCycle.Instance.GetNextCamera(RoleNeeded, currentCam);
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
