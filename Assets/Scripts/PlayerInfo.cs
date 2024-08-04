using System;
using UnityEngine;
using Mirror;

public class PlayerInfo : NetworkBehaviour
{
    /// <summary>
    /// Meant to be hold information that will be more relevant when the game becomes multiplayer
    /// </summary>
    public string PlayerName { get; private set; }
    public Role CurrentRole { get; private set; }
    public delegate void RoleChangeDelegate(Role oldRole, Role newRole);
    public RoleChangeDelegate OnRoleChange;

    public void PickRole(Role role)
    {
        //This will later need checks to make sure the picked role isn't over the limit
        Role oldRole = CurrentRole;
        CurrentRole = role;
        CamCycle.Instance.ChangeRoles(oldRole, role);
        Debug.Log($"Changed role from {oldRole.Name} to {role.Name}");
        if (OnRoleChange != null)
        {
            OnRoleChange(oldRole, role);
        }
    }

    public void PickName(string name)
    {
        PlayerName = name;
    }

    public void Awake()
    {
        CurrentRole = CrewRoles.Driver; //For now we'll always start you as the driver during our testing
    }

    public void OnDebugChangeRoles()
    {
        int currentRoleIndex = Array.IndexOf(CrewRoles.ImplementedRoles, CurrentRole);
        currentRoleIndex++; //Increment to get the next role
        if (currentRoleIndex >= CrewRoles.ImplementedRoles.Length)
            currentRoleIndex = 0;

        PickRole(CrewRoles.ImplementedRoles[currentRoleIndex]); //Cycle the role over to the newly picked one
    }
}
