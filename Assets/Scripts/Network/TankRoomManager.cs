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

    [SerializeField] GameObject lobbyUIPrefab; //Try spawning the prefab instead of leaving it in the scene?
    [SerializeField] GameObject chatroomPrefab;
    [SerializeField] GameObject enemyManagerPrefab; //I'm going to attempt to spawn the enemy stuff in here from the tank manager
    [SerializeField] GameObject horniTankPrefab; //For spawning after the game scene is loaded
    [SerializeField, ReadOnly] RolePicker rolePicker; //Cached after the UI is made
    [SerializeField] bool startInDebugMode = true;

    Chat chatroom; //For sending join and disconnect messages on

    /// <summary>
    /// A gameobject that allows the manager to send messages back and forth. I dislike that the NetworkManager can't do that on it's own.
    /// </summary>
    public RoomNetworking RoomNetworking;

    public Dictionary<NetworkIdentity, PlayerProfile> connectedPlayers = new(); //Anybody added to the server gets stashed in here for me to use instead of Mirror's stuff. I know it's duplicated, but let me cook

    public Dictionary<ProfileGroup, GameObject> GroupToVehicles = new(); //This is for supporting multiple tanks later on

    public static new TankRoomManager singleton => NetworkManager.singleton as TankRoomManager;

    public override void ReadyStatusChanged()
    {
        base.ReadyStatusChanged();
        //Debug.Log("Player is now ready");
    }

    /// <summary>
    /// Handles creation of the game prefab and copies over information that was held in the PlayerProfile from the RoomPlayer.
    /// </summary>
    /// <param name="conn"></param>
    /// <param name="roomPlayer"></param>
    /// <returns></returns>
    public override GameObject OnRoomServerCreateGamePlayer(NetworkConnectionToClient conn, GameObject roomPlayer)
    {
        //Called on the server when creating the prefab for the player
        // get start position from base class
        Transform startPos = GetStartPosition();
        GameObject gamePlayer = startPos != null
            ? Instantiate(playerPrefab, startPos.position, startPos.rotation)
            : Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);

        PlayerProfile gameProfile = gamePlayer.GetComponent<ProfileHolder>().Profile;
        PlayerProfile roomProfile = roomPlayer.GetComponent<ProfileHolder>().Profile;

        //The gameProfile needs to have all its info copied over from the roomProfile
        //For now this is just the player's name and their role
        gameProfile.PlayerName = roomProfile.PlayerName;
        //HEY FUTURE VICKY! ADD ROLE STUFF TO THE PLAYERPROFILE, YOU DUMB BITCH!

        //Yes ma'am, role stuff is added now <3
        gameProfile.SelectRole(roomProfile.CurrentRole); //Assign the game profile to have the same role as the room profile

        return gamePlayer; //Send the player prefab back
    }

    public override void OnRoomServerDisconnect(NetworkConnectionToClient conn)
    {
        //I do not need to call the base implementation as it is virtual
        if (conn.identity != null)
        { //Handle a proper disconnect
            if (connectedPlayers.ContainsKey(conn.identity))
            { //We have properly connected them before
                PlayerProfile profile = connectedPlayers[conn.identity]; //Grab the profile for the disconnect chat message
                chatroom.SendServerMessage($"{profile.PlayerName} has disconnected!", new Chat.MessageContext(Chat.MessageTypes.SERVER, false, true)); 
                //TODO: Check if we need to do any further cleanup on other clients. This should be it for now
                if (Utils.IsSceneActive(RoomScene))
                { //If a player disconnects in the middle of role picking, we should free up their role if they picked one
                    if (profile.CurrentRole != CrewRoles.UnassignedRole)
                    {
                        //Role cleanup in here.
                        //Tell the rolePicker to set this player back to unassigned and free up their role
                        rolePicker.CmdSelectRole(CrewRoles.UnassignedRole, conn.identity);
                        //And for now, that appears to be it for cleanup
                    }
                }
                connectedPlayers.Remove(conn.identity); //Take the player out of the connected list now that they're gone
            }
            else
            { //Otherwise something weird is going on and we should let the programmer know
                Debug.LogWarning("Attempted to disconnect player with valid identity, but that was not set up in TankRoomManager.connectedPlayers. Investigate this.");
            }
        }
        else
        { //Handle a connection that didn't go well
            Debug.Log("Disconnected a client that had no connection.identity set.");
        }
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);
        //Debug.Log("Server added a player with a netId of " + conn.identity.netId);
    }

    public override void OnRoomServerSceneChanged(string sceneName)
    {
        if (sceneName == RoomScene)
        {
            //Debug.Log("Server moved into room scene, spawning canvas");
            GameObject LobbyUI = GameObject.Instantiate(lobbyUIPrefab);
            chatroom = LobbyUI.GetComponentInChildren<Chat>(); //Finds the chat component inside the lobby ui
            rolePicker = LobbyUI.GetComponent<RolePicker>();
            NetworkServer.Spawn(LobbyUI);
            //Let me spawn the RoomNetworking over the network too, just to be safe
            GroupToVehicles.Clear(); //Make sure this is empty when a room starts
            GroupToVehicles.Add(new ProfileGroup(), null); //The gameobject is null until we go into the game scene
        }
        if (sceneName == GameplayScene)
        {
            //Time to spawn the tank in
            //We'll worry about picking a proper spawn point later on
            
            //Since the HorniTank is a syncvar we shouldn't need to manually invoke the event handler. This should just work
            GameObject tank = GameObject.Instantiate(horniTankPrefab); //Double check this puts the tank at 0,0,0
            NetworkServer.Spawn(tank); //Spawn the tank across the server for everyone
            RoomNetworking.HorniTank = tank;
            Debug.Log($"Tank var is {tank} and RoomNetworking is {RoomNetworking} with its HorniTank set to {RoomNetworking.HorniTank}");

            GameObject canvas = GameObject.Find("Canvas");
            GameObject chatroomObject = GameObject.Instantiate(chatroomPrefab, canvas.transform);
            chatroom = chatroomObject.GetComponent<Chat>();
            NetworkServer.Spawn(chatroomObject);
            if (startInDebugMode)
                canvas.SetActive(false); //Turn the UI off when we're in debug mode
            GameObject enemyManager = GameObject.Instantiate(enemyManagerPrefab); //Now spawn across the network
            NetworkServer.Spawn(enemyManager);
        }
    }

    /// <summary>
    /// When a client connects to the game they send their profile across the network in TankRoomPlayer. This is called from TankRoomPlayer.CmdSendProfile so the server has this information.
    /// </summary>
    /// <param name="identity">The networkidentity that owns this profile</param>
    /// <param name="profile">The player profile to be saved on the server</param>
    public void AddProfileToRoom(NetworkIdentity identity, PlayerProfile profile)
    {
        if (connectedPlayers.ContainsKey(identity))
        { //If the identity already exists in the dictionary then we're merely swapping out their profile for the new one
            connectedPlayers[identity] = profile;
        }
        else
        {
            connectedPlayers.Add(identity, profile); //Put them in the connected dictionary
            ProfileGroup group = GroupToVehicles.Keys.First();
            group.Group.Add(profile); //Stick their profile in the one and only group we have for now
        }
        //TODO: I want to add random connection messages like discord does with people joining a server. There is a trello card for this request.
        chatroom.SendServerMessage($"{profile.PlayerName} has connected!", new Chat.MessageContext(Chat.MessageTypes.SERVER, true, false));
    }

    public override void OnGUI()
    {
        base.OnGUI();
    }

}
 