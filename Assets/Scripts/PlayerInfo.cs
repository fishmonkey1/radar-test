using System;
using UnityEngine;

public class PlayerInfo : MonoBehaviour
{
    /// <summary>
    /// Meant to be hold information that will be more relevant when the game becomes multiplayer
    /// </summary>
    public string PlayerName { get; private set; }
    public Role CurrentRole { get; private set; }
    static PlayerInfo instance; //Hold the singleton during debug testing and remove this when multiplayer is added
    public static PlayerInfo Instance => instance; //Return the singleton
    public delegate void RoleChangeDelegate(Role oldRole, Role newRole);
    public RoleChangeDelegate OnRoleChange;

    public void PickRole(Role role)
    {
        //This will later need checks to make sure the picked role isn't over the limit
        Role oldRole = CurrentRole;
        CurrentRole = role;
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
        if (instance != null)
        {
            throw new System.Exception("You have more than one PlayerInfo component in the scene!");
        }
        instance = this; //Lazy singleton instantiation for now
        PickRole(CrewRoles.Driver); //For now we'll always start you as the driver during our testing
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
