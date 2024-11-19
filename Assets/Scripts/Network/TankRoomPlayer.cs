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

    public uint RoleID = CrewRoles.UnassignedRole.ID; //Init to the UnassignedRole
    public Role role = CrewRoles.UnassignedRole; //All players are created with the UnassignedRole

    public override void Start()
    {
        base.Start();
        //Debug.Log($"IsLocalPlayer equals {isLocalPlayer}");
        //If you are the local player, we need to either load your profile or create it
        if (isLocalPlayer)
        {
            PlayerProfile profile = GetComponent<PlayerProfile>();
            if (PlayerProfile.TryLoadProfile(Application.persistentDataPath + "/PlayerProfiles", PlayerProfile.LoadedProfile, out profile))
            {
                Debug.Log($"Loaded player profile with name: {profile.PlayerName}");
            }
            else
            { //If we couldn't find your profile, let's assume it's new and export it
                profile.PlayerName = PlayerProfile.LoadedProfile; //Technically that's all that needs to be done for now
                profile.ExportToJson(Application.persistentDataPath + "/PlayerProfiles");
            }
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
        if (RoleID == CrewRoles.UnassignedRole.ID)
            return false; //You're on the unassigned role
        if (role != null)
            return true;
        return false;
    }

    public override void OnGUI()
    {
        base.OnGUI();
    }

}
