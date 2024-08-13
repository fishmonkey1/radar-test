using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class TankRoomPlayer : NetworkRoomPlayer
{
    /// Inherit from the NetworkRoomPlayer and take a look at using the hooks or overriding one of the funtions
    /// Needs to track the role the player picked and assign that info to PlayerInfo
    /// Sends a message to the server when they've picked a role so everybody gets updated on which are left
    /// Can't ready up til you've picked a role

    [SyncVar (hook=nameof(PickRole))] public uint RoleID = 999; //Init to an invalid role since 0 aligns with the first
    [SyncVar] public string PlayerName = null;
    Role role;

    [Command]
    public void CmdPickRole(uint roleID)
    { //Should be it for now boss
        role = CrewRoles.GetRoleByID(roleID);
        RoleID = roleID;
    }

    private void PickRole(uint oldID, uint newID) 
    {
        Role newRole = CrewRoles.GetRoleByID(newID);
        if (newRole != role)
        {
            role = newRole;
        }
    }

    public bool HasAnyRole()
    {
        if (role != null)
            return true;
        return false;
    }

    public void CmdSetName(string newName)
    {
        PlayerName = newName; //And that's pretty much it
    }

    public override void OnGUI()
    {
        base.OnGUI();
    }

}
