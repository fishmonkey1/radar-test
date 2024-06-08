using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Turret : MonoBehaviour, IRoleNeeded
{

    [SerializeField]
    Transform turret; //The turret moves along the z axis
    [SerializeField]
    Transform barrel; //The barrel moves along the x axis
    [SerializeField]
    float rotationSensitivity = 1f; //For determining how quickly the turret rotates with user input
    [SerializeField] GameObject projectilePrefab; //This will need to be fetched later when there are different ammos
    [SerializeField] Transform projectileSpawn;

    Vector2 turretInput = Vector2.zero;
    Camera currentCam;

    public Role RoleNeeded => CrewRoles.Gunner;

    public void OnMove(InputValue input)
    {
        if (!((IRoleNeeded)this).HaveRole(PlayerInfo.Instance.CurrentRole))
            return; //Don't allow turret inputs if you don't have the gunner role selected
        turretInput = input.Get<Vector2>();
    }

    public void OnFire()
    {
        if (!((IRoleNeeded)this).HaveRole(PlayerInfo.Instance.CurrentRole))
            return;
        //TODO: Handle shooting logic here next time
        GameObject projectileObject = GameObject.Instantiate(projectilePrefab, projectileSpawn.position, Quaternion.identity);
        Projectile projectile = projectileObject.GetComponent<Projectile>();
        projectile.shooter = gameObject;
        projectileObject.transform.rotation = Quaternion.LookRotation(projectileSpawn.forward, projectileSpawn.up);
    }

    public void OnRoleChange(Role oldRole, Role newRole)
    {
        if (newRole != RoleNeeded) return;

        //Otherwise we do any setup in here
        currentCam = CamCycle.Instance.GetFirstCamera(RoleNeeded);
        Debug.Log($"Got first camera for {RoleNeeded.Name} role");
    }

    public void OnCameraToggle()
    {
        if (!((IRoleNeeded)this).HaveRole(PlayerInfo.Instance.CurrentRole))
            return;

        currentCam = CamCycle.Instance.GetNextCamera(RoleNeeded, currentCam);
    }

    void Start()
    {
        if (PlayerInfo.Instance.OnRoleChange == null)
        {
            PlayerInfo.Instance.OnRoleChange = new PlayerInfo.RoleChangeDelegate(OnRoleChange);
            Debug.Log("Assigned Turret RoleChange to new delegate");
        }
        else
        {
            PlayerInfo.Instance.OnRoleChange += OnRoleChange;
            Debug.Log("Assigned Turret RoleChange to existing delegate");
        }
    }

    void Update()
    {
        //TODO: Do turret translation in here next time I code
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
