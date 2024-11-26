using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Profiling;

public class TankRoomPlayer : NetworkRoomPlayer
{
    /// Inherit from the NetworkRoomPlayer and take a look at using the hooks or overriding one of the funtions
    /// Needs to track the role the player picked and assign that info to the PlayerProfile
    /// Sends a message to the server when they've picked a role so everybody gets updated on which are left
    /// Can't ready up til you've picked a role

    public PlayerProfile playerProfile;
    public Role role = CrewRoles.UnassignedRole;

    public override void OnClientEnterRoom()
    {
        base.OnClientEnterRoom();
        Debug.Log($"IsLocalPlayer equals {isLocalPlayer}");
        //If you are the local player, we need to either load your profile or create it
        if (isLocalPlayer)
        {
            ProfileHolder holder = GetComponent<ProfileHolder>();
            playerProfile = holder.Profile; //Get the profile off of the holder
            PlayerProfile attemptLoad;
            if (PlayerProfile.TryLoadProfile(Application.dataPath + "/JSON/PlayerProfiles/", PlayerProfile.LoadedProfileName, out attemptLoad))
            {
                Debug.Log($"Loaded player profile with name: {playerProfile.PlayerName}");
                playerProfile = attemptLoad;
                holder.Profile = playerProfile;
            }
            else
            { //If we couldn't find your profile, let's assume it's new and export it
                Debug.Log("LoadedProfileName: " + PlayerProfile.LoadedProfileName);
                Debug.Log("playerProfile: " + playerProfile);
                playerProfile.PlayerName = PlayerProfile.LoadedProfileName; //Technically that's all that needs to be done for now
                playerProfile.ExportToJson(Application.dataPath + "/JSON/PlayerProfiles/");
                Debug.Log($"Exported player profile to path: {Application.dataPath + "/JSON/PlayerProfiles/"}");
            }
            playerProfile.Holder = holder;
            holder.Profile = playerProfile;
        }
        //Now that our profile is set up, we need to let the server know
        Debug.Log("Value of playerProfile is: " + playerProfile + " and profile is named " + playerProfile.PlayerName);
        CmdSendProfile(playerProfile, NetworkClient.localPlayer);
    }

    [Command]
    public void CmdSendProfile(PlayerProfile profile, NetworkIdentity identity)
    { //At the moment, the playername is the only thing in a profile. Later this needs to send unlocks and experience as well
        PlayerProfile serverProfile = identity.GetComponent<ProfileHolder>().Profile; //Fetch that player's component on the server
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
            PlayerProfile localProfile = identity.GetComponent<ProfileHolder>().Profile;
            localProfile.PlayerName = profile.PlayerName; //Set the name to match what was sent
            //Later this will read in the experience and other sections of the profile
            Debug.Log($"Received broadcasted profile named {profile.PlayerName} and updated their component");
        }
    }

    [Command]
    public void CmdPickRole(uint roleID)
    {
        role = CrewRoles.GetRoleByID(roleID);
    }

    public void PickRole(uint oldID, uint newID) 
    {
        Role newRole = CrewRoles.GetRoleByID(newID);
        if (newRole != role)
        {
            role = newRole;
        }
    }

    public bool HasAnyRole()
    {
        if (role == CrewRoles.UnassignedRole)
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
