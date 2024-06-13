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

    [Command]
    private void CmdPickRole(uint roleID)
    { //Should be it for now boss
        TankRoomManager tankRoom = NetworkManager.singleton as TankRoomManager; //Cast to TankRoom
        tankRoom.PlayerPickedRole(CrewRoles.GetRoleByID(roleID), this.connectionToClient);
    }

}
