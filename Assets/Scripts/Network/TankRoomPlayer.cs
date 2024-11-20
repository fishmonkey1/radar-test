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

    PlayerProfile playerProfile;

    public override void Start()
    {
        base.Start();
        //Debug.Log($"IsLocalPlayer equals {isLocalPlayer}");
        //If you are the local player, we need to either load your profile or create it
        if (isLocalPlayer)
        {
            PlayerProfile profile = GetComponent<PlayerProfile>();
            if (PlayerProfile.TryLoadProfile(Application.persistentDataPath + "/PlayerProfiles", PlayerProfile.LoadedProfileName, out profile))
            {
                Debug.Log($"Loaded player profile with name: {profile.PlayerName}");
            }
            else
            { //If we couldn't find your profile, let's assume it's new and export it
                profile.PlayerName = PlayerProfile.LoadedProfileName; //Technically that's all that needs to be done for now
                profile.ExportToJson(Application.persistentDataPath + "/PlayerProfiles");
                Debug.Log($"Exported player profile to path: {Application.persistentDataPath + "/PlayerProfiles"}");
            }
            //Assign our profile to our player now
            playerProfile = profile;
            //Now that our profile is set up, we need to let the server know
            CmdSendProfile(profile, NetworkClient.localPlayer); 
        }
    }

    [Command]
    public void CmdSendProfile(PlayerProfile profile, NetworkIdentity identity)
    { //At the moment, the playername is the only thing in a profile. Later this needs to send unlocks and experience as well
        PlayerProfile serverProfile = identity.GetComponent<PlayerProfile>(); //Fetch that player's component on the server
        serverProfile.PlayerName = profile.PlayerName; //Update their name on the server
        TankRoomManager room = TankRoomManager.singleton;
        room.AddProfileToRoom(identity, serverProfile);
        //Technically that's all, folks. Now we tell all the clients about the new profile
        RpcBroadcastProfile(profile, identity);
    }

    [ClientRpc]
    public void RpcBroadcastProfile(PlayerProfile profile, NetworkIdentity identity)
    { //The server runs this on all connected clients, excluding the person who sent the message
        if (string.Equals(PlayerProfile.LoadedProfileName, profile.PlayerName))
        {
            Debug.Log("Received broadcasted profile, but it matches our loaded profile.");
        }
        else
        { //Otherwise we have somebody else's profile, so lets update things
            PlayerProfile localProfile = identity.GetComponent<PlayerProfile>();
            localProfile.PlayerName = profile.PlayerName; //Set the name to match what was sent
            //Later this will read in the experience and other sections of the profile
            Debug.Log($"Received broadcasted profile named {profile.PlayerName} and updated their component");
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
