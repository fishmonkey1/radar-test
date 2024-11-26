using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;

/// <summary>
/// This class implements all of the controls for driving a tank around. See <see cref="IRoleNeeded"/> for how the PlayerProfile is used to limit which controls are read.
/// TODO: The controls will be separated into different mappings later, which means this will also need an update.
/// </summary>
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

    /// <summary>
    /// Implementation of <see cref="IRoleNeeded"/> that only allows a PlayerProfile with the Driver role to send inputs
    /// </summary>
    public Role RoleNeeded => CrewRoles.Driver;
    /// <summary>
    /// The PlayerProfile that this script is assigned to
    /// </summary>
    PlayerProfile playerProfile;

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

    /// <summary>
    /// Called from the PlayerProfile script and sets tankSteer up to actually listen to their inputs.
    /// </summary>
    /// <param name="profile">The profile that owns this script</param>
    public void SetPlayer(PlayerProfile profile)
    {
        Debug.Log("Assigning local player to tankSteer. info's role is " + profile.CurrentRole);
        if(RoleNeeded == profile.CurrentRole)
        {
            Debug.Log("Player's role matches for tankSteer");
            currentCam = CamCycle.Instance.GetFirstCamera(RoleNeeded);
        }
        playerProfile = profile;
        if (playerProfile.OnRoleChange == null)
            playerProfile.OnRoleChange = new PlayerProfile.RoleChangeDelegate(OnRoleChange);
        else
            playerProfile.OnRoleChange += OnRoleChange;
    }

    /// <summary>
    /// Handle moving the actual tank around in the scene. It's implied that this only happens on the host, as the inputs are routed to the server and then the position sent back to them.
    /// </summary>
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

    /// <summary>
    /// Finds the correct camera for this role when the <see cref="PlayerProfile.OnRoleChange"/> delegate gets called.
    /// </summary>
    /// <param name="oldRole">The role the profile had last.</param>
    /// <param name="newRole">The role the profile has now.</param>
    public void OnRoleChange(Role oldRole, Role newRole)
    {
        if (newRole != RoleNeeded) return;

        //Otherwise we do any setup in here
        currentCam = CamCycle.Instance.GetFirstCamera(RoleNeeded); //Fetch the camera for the driver so it's active
        Debug.Log($"Got first camera for {RoleNeeded.Name} role");
    }

    /// <summary>
    /// Captures input from Unity's InputSystem module and sends them to the server to be performed there unless they are the host. This method only allows players with the correct role to send these messages.
    /// </summary>
    /// <param name="value">The Vector2 value from the input system.</param>
    public void OnMove(InputValue value)
    {
        if (playerProfile == null)
            return; //This role is unused, so do nothing
        if (!((IRoleNeeded)this).HaveRole(playerProfile.CurrentRole))
            return; //Don't allow driving inputs if you don't have the driver role selected
        if (isServer) //Only apply locally if you are the host
            driverInput = value.Get<Vector2>();
        else
            CmdSendInputs(value.Get<Vector2>()); //Otherwise send to the server
    }

    /// <summary>
    /// Sent from clients to the server so it can be executed there and the results sent back to them.
    /// </summary>
    /// <param name="input">A Vector2 sent from <see cref="OnMove(InputValue)"/></param>
    [Command(requiresAuthority = false)]
    public void CmdSendInputs(Vector2 input)
    {
        //Set the inputs on the server and then let it replicate the tank's position for everyone else
        driverInput.x = input.x;
        driverInput.y = input.y;
    }

    /// <summary>
    /// InputSystem capture for the cycle camera button. Calls CamCycle to get the next camera and assigns it to our currentCam.
    /// </summary>
    public void OnCameraToggle()
    {
        if (playerProfile == null)
            return; //This role is unused, so do nothing
        if (!((IRoleNeeded)this).HaveRole(playerProfile.CurrentRole))
            return;

        currentCam = CamCycle.Instance.GetNextCamera(RoleNeeded, currentCam);
    }

    /// <summary>
    /// TODO: Consider moving this elsewhere...
    /// InputSystem capture for the ToggleRadar button which shows/hides the minimap in the corner.
    /// </summary>
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
