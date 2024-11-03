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

    [SyncVar (hook=nameof(PickRole))] public uint RoleID = CrewRoles.UnassignedRole.ID; //Init to the UnassignedRole
    [SyncVar] public string PlayerName = null;
    public Role role = CrewRoles.UnassignedRole; //All players are created with the UnassignedRole

    public override void Start()
    {
        base.Start();
        //Debug.Log($"IsLocalPlayer equals {isLocalPlayer}");
        if (isLocalPlayer)
        {
            PlayerName = PlayerInfo.localPlayerName;
            GetComponent<PlayerInfo>().PickName(PlayerName);
            CmdSetName(PlayerInfo.localPlayerName);
        }
    }

    [Command]
    public void CmdPickRole(uint roleID)
    {
        role = CrewRoles.GetRoleByID(roleID);
        RoleID = roleID;
    }

    public void PickRole(uint oldID, uint newID) 
    {
        Role newRole = CrewRoles.GetRoleByID(newID);
        if (newRole != role)
        {
            role = newRole;
            RoleID = newRole.ID;
        }
    }

    public bool HasAnyRole()
    {
        if (RoleID == 999)
            return false; //You're on the default role
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
