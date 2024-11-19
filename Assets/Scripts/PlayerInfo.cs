using System;
using UnityEngine;
using Mirror;

public class PlayerInfo : NetworkBehaviour
{
    /// <summary>
    /// Meant to be hold information that will be more relevant when the game becomes multiplayer
    /// </summary>
    [SyncVar] public string PlayerName;
    [SyncVar(hook = nameof(NetworkChangeRole))] public Role CurrentRole;
    public delegate void RoleChangeDelegate(Role oldRole, Role newRole);
    public RoleChangeDelegate OnRoleChange;
    public GameObject horniTank;

    public static string localPlayerName = "default"; //This lets the local player assign their name

    void Start()
    {
        Debug.Log($"PlayerInfo is running its start function. isLocalPlayer equals: {isLocalPlayer}");
        if (Utils.IsSceneActive(TankRoomManager.singleton.RoomScene) && isLocalPlayer)
        { //We've moved into the room scene and should have all our components ready for use. Lets get the PlayerInfo component off the client and tell the server about them
            PlayerName = localPlayerName; //I forgor to do this... Now they'll start off with the correct name they picked
            NetworkRegisterPlayerInfo();
        }
    }

    public override void OnStartLocalPlayer()
    {
        Debug.Log("Starting up a local PlayerInfo component");
        if (isLocalPlayer && Utils.IsSceneActive(TankRoomManager.singleton.GameplayScene))
        {
            Debug.Log("This PlayerInfo is on a local player and we are in the gameplay screen");
            horniTank = GameObject.FindGameObjectWithTag("PlayerTank"); //Find the tank and assign it
            PickRole(CurrentRole); //Ensure that we have the correct role at the start of the match
        }

    }

    /// <summary>
    /// This function is called by the server when it changes the syncVar for CurrentRole
    /// </summary>
    /// <param name="oldRole">The last role the player had, if any. Can be null.</param>
    /// <param name="newRole">The role that's being assigned to the PlayerInfo</param>
    public void NetworkChangeRole(Role oldRole, Role newRole)
    {
        Debug.Log("SyncVar for Role changed. Picking new role.");
        Debug.Log($"oldRole is {oldRole.Name} and newRole is {newRole.Name}");
        PickRole(newRole);
    }

    /// <summary>
    /// Called from TankRoomManager after joining the lobby scene. This calls a command so the server gets the local information, and that command calls a RPC so all connected clients get updated as to who is in the game.
    /// </summary>
    public void NetworkRegisterPlayerInfo()
    {
        CmdRegisterPlayerInfo(this);
    }

    [Command(requiresAuthority = false)]
    public void CmdRegisterPlayerInfo(PlayerInfo info, NetworkConnectionToClient conn = null)
    {
        TankRoomManager room = TankRoomManager.singleton; //Nab the server's RoomManager
        //room.AddPlayerToRoom(conn, info); //And assign the player to the room
        //Finally, we broadcast the registration to all the clients
        //RpcBroadcastRegistration(info);
    }

    /*[ClientRpc]
    public void RpcBroadcastRegistration(PlayerInfo info) 
    { //This code is run on the clients, so we should check the connection identity to make sure we aren't getting a message that this client sent, since that would duplicate our efforts
        NetworkConnection conn = 
        TankRoomManager room = TankRoomManager.singleton;
        if (!room.connectedPlayers.ContainsKey(conn))
        { //We are not the client that sent this message, so lets update our local stuff
            room.AddPlayerToRoom(conn, info); //Register them in our local room manager
        }
        else
            Debug.Log("Received registration broadcast, but it originated from this client, so ignoring it.");
    }*/

    public void PickRole(Role role)
    {
        //This will later need checks to make sure the picked role isn't over the limit
        Role oldRole = CurrentRole;
        CurrentRole = role;
        if (Utils.IsSceneActive(TankRoomManager.singleton.GameplayScene))
        {
            bool isLocal = GameObject.ReferenceEquals(gameObject, NetworkClient.localPlayer.gameObject);
            if (isLocal)
            {
                CamCycle.Instance.ChangeRoles(oldRole, role);
                if (horniTank != null)
                {
                    Debug.Log("Assigning player to spawned tank");
                    if (role.Name == CrewRoles.Gunner.Name)
                        horniTank.GetComponent<Turret>().SetPlayer(this);
                    if (role.Name == CrewRoles.Driver.Name)
                        horniTank.GetComponent<tankSteer>().SetPlayer(this);
                }
            }
        }
        
        if (oldRole == null)
            Debug.Log($"Assigned role named {role.Name} to player");
        else
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

    public void OnDebugChangeRoles()
    {
        int currentRoleIndex = Array.IndexOf(CrewRoles.ImplementedRoles, CurrentRole);
        currentRoleIndex++; //Increment to get the next role
        if (currentRoleIndex >= CrewRoles.ImplementedRoles.Length)
            currentRoleIndex = 0;
        //We just skip the unassigned role, since I'd probably set up some different debug stuff for testing that
        if (currentRoleIndex == CrewRoles.UnassignedRole.ID)
            currentRoleIndex++;

        PickRole(CrewRoles.ImplementedRoles[currentRoleIndex]); //Cycle the role over to the newly picked one
    }
}
