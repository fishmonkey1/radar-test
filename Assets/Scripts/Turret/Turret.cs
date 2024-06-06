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

    Vector2 turretInput = Vector2.zero;

    public Role RoleNeeded => CrewRoles.Gunner;

    public void OnMove(InputValue input)
    {
        if (!((IRoleNeeded)this).HaveRole(PlayerInfo.Instance.CurrentRole))
            return; //Don't allow turret inputs if you don't have the gunner role selected
        turretInput = input.Get<Vector2>();
    }

    public void OnFire()
    {
        //TODO: Handle shooting logic here next time
    }

    void Update()
    {
        //TODO: Do turret translation in here next time I code
    }

}
