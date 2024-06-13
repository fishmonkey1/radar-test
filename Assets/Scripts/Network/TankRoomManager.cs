using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;

public class TankRoomManager : NetworkRoomManager
{
    /// Subclass the NetworkRoomManager so it takes into account the roles that are able to be picked
    /// Don't let players ready up unless they have a role picked
    /// Figure out how I'm linking up roles with their associated scripts at some point :c

    readonly SyncDictionary<Role, NetworkConnection> pickedRoles = new();

    public void PlayerPickedRole(Role role, NetworkConnection conn)
    { //Called on the server from TankRoomPlayer when a role is selected
        if (pickedRoles.ContainsKey(role))
        {
            Debug.LogWarning("Tried to pick a role that is already picked by someone else!");
            return;
        }
        pickedRoles.Add(role, conn); //Match the player's connection to the role

    }
    
}
 