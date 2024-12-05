using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;

/// <summary>
/// Reads all of the controls for the turret and works over the network. See <see cref="IRoleNeeded"/>
/// </summary>
public class Turret : NetworkBehaviour, IRoleNeeded
{

    [SerializeField]
    Transform turret; //The turret moves along the z axis
    [SerializeField]
    Transform barrel; //The barrel moves along the x axis
    [SerializeField]
    float rotationSensitivity = 1f; //For determining how quickly the turret rotates with user input
    [SerializeField] GameObject projectilePrefab; //This will need to be fetched later when there are different ammos
    [SerializeField] Transform projectileSpawn;

    [SerializeField, ReadOnly]Vector2 turretInput = Vector2.zero;
    Camera currentCam;
    PlayerProfile playerProfile; //The player that has the Gunner role goes here

    /// <summary>
    /// Set the role needed to read controls to the statically defined Gunner role.
    /// </summary>
    public Role RoleNeeded => CrewRoles.Gunner;

    /// <summary>
    /// Sent from the input system, which moves the turret around.
    /// </summary>
    /// <param name="input"></param>
    public void OnMove(InputValue input)
    {
        if (playerProfile == null)
            return; //This role is unused, so do nothing
        if (!((IRoleNeeded)this).HaveRole(playerProfile.CurrentRole))
            return; //Don't allow turret inputs if you don't have the gunner role selected
        if (isServer) //Only move the turret locally and replicate if you're the host
            turretInput = input.Get<Vector2>();
        else //Otherwise we send our input commands to the server
            CmdOnMove(input.Get<Vector2>());
    }

    /// <summary>
    /// If the local player isn't the host, we instead send our inputs to be done on the host.
    /// </summary>
    /// <param name="input"></param>
    [Command(requiresAuthority=false)]
    public void CmdOnMove(Vector2 input)
    {  //The transforms are synced across the network so this replicates to all clients
        turretInput.x = input.x;
        turretInput.y = input.y;
    }

    /// <summary>
    /// Sent from the input system, which fires the turret.
    /// TODO: Create an inventory or ammo component so we can't fire forever.
    /// </summary>
    public void OnFire()
    {
        if (playerProfile == null)
        {
            Debug.Log("No PlayerProfile on turret to call OnFire with.");
            return; //This role is unused, so do nothing
        }
        if (!((IRoleNeeded)this).HaveRole(playerProfile.CurrentRole))
        {
            Debug.Log("Ignoring fire order from Non-Gunner player");
            return;
        }
        CmdOnFire();
    }

    /// <summary>
    /// If the local player isn't the host, we instead route our inputs to the host.
    /// </summary>
    [Command(requiresAuthority=false)]
    public void CmdOnFire()
    {
        Debug.Log("Server is spawning projectile");
        GameObject projectileObject = GameObject.Instantiate(projectilePrefab, projectileSpawn.position, Quaternion.identity);
        Projectile projectile = projectileObject.GetComponent<Projectile>();
        projectile.shooter = gameObject;
        projectileObject.transform.rotation = Quaternion.LookRotation(projectileSpawn.forward, projectileSpawn.up);
        NetworkServer.Spawn(projectileObject);
    }

    /// <summary>
    /// Setup the cameras for this role if the newRole matches, otherwise clean up if this is the old role.
    /// </summary>
    /// <param name="oldRole"></param>
    /// <param name="newRole"></param>
    public void OnRoleChange(Role oldRole, Role newRole)
    {
        if (newRole != RoleNeeded) return;

        //Otherwise we do any setup in here
        currentCam = CamCycle.Instance.GetFirstCamera(RoleNeeded);
        Debug.Log($"Got first camera for {RoleNeeded.Name} role");
    }

    /// <summary>
    /// Ask the CamCycle script for the next camera.
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
    /// Set when the tank is spawned, this allows for the local player to control this script.
    /// </summary>
    /// <param name="profile"></param>
    public void SetPlayer(PlayerProfile profile)
    {
        Debug.Log("Assigning local player in Turret.");
        playerProfile = profile;
        if (RoleNeeded.Name == profile.CurrentRole.Name)
        {
            Debug.Log("Local player's role matches for Turret");
            currentCam = CamCycle.Instance.GetFirstCamera(RoleNeeded);
        }
        if (playerProfile.OnRoleChange == null)
        {
            playerProfile.OnRoleChange = new PlayerProfile.RoleChangeDelegate(OnRoleChange);
        }
        else
        {
            playerProfile.OnRoleChange += OnRoleChange;
        }
    }

    void Update()
    {
        if (!Mathf.Approximately(0, turretInput.x))
        { //Only turn the turret if we have input to use
            turret.Rotate(Vector3.forward, turretInput.x * rotationSensitivity * Time.deltaTime);
        }

        if (!Mathf.Approximately(0, turretInput.y))
        {
            barrel.Rotate(Vector3.left, turretInput.y * rotationSensitivity * Time.deltaTime);
        }

    }

}
