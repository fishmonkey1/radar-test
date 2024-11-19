using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

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

    public Dictionary<NetworkIdentity, PlayerProfile> connectedPlayers = new(); //Anybody added to the server gets stashed in here for me to use instead of Mirror's stuff. I know it's duplicated, but let me cook

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

        PlayerProfile gameProfile = gamePlayer.GetComponent<PlayerProfile>();
        PlayerProfile roomProfile = roomPlayer.GetComponent<PlayerProfile>();

        //The gameProfile needs to have all its info copied over from the roomProfile
        //For now this is just the player's name and their role
        gameProfile.PlayerName = roomProfile.PlayerName;
        //HEY FUTURE VICKY! ADD ROLE STUFF TO THE PLAYERPROFILE, YOU DUMB BITCH!

        //Yes ma'am, role stuff is added now <3
        gameProfile.SelectRole(roomProfile.CurrentRole); //Assign the game profile to have the same role as the room profile


        //This is all trash now
        //newInfo.PickRole(CrewRoles.GetRoleByID(roomInfo.RoleID)); //Set the role to what was in the room player
        //newInfo.PickName(roomInfo.PlayerName);

        return gamePlayer; //Send the player prefab back
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);
        //Debug.Log("Server added a player with a netId of " + conn.identity.netId);
    }

    public override void OnRoomStopClient()
    {
        base.OnRoomStopClient();
    }

    public override void OnRoomStopServer()
    {
        base.OnRoomStopServer();
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
        }
        if (sceneName == GameplayScene)
        {
            //Time to spawn the tank in
            //We'll worry about picking a proper spawn point later on
            //Debug.Log("Server moved into gameplay scene, spawning tank");
            GameObject horniTank = GameObject.Instantiate(horniTankPrefab); //Double check this puts the tank at 0,0,0
            NetworkServer.Spawn(horniTank);
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
    /// <param name="identity"></param>
    /// <param name="profile"></param>
    public void AddProfileToRoom(NetworkIdentity identity, PlayerProfile profile)
    {
        if (connectedPlayers.ContainsKey(identity))
        { //If the identity already exists in the dictionary then we're merely swapping out their profile for the new one
            connectedPlayers[identity] = profile;
        }
        else
        {
            connectedPlayers.Add(identity, profile);
        }
    }

    public void AddPlayerToRoom(NetworkConnection conn, PlayerInfo info)
    {
        //This will be Vicky's spot to set up all the information on freshly joined clients.
        connectedPlayers.Add(conn, info); //Place the info in our dictionary
        if (NetworkClient.localPlayer.isServer)
        { //This call is happening on the host, so we can go ahead and broadcast our connection message to everyone
            Debug.Log($"Adding player {info.PlayerName} to room on the host.");
            chatroom.SendServerMessage($"{info.PlayerName} connected", Chat.MessageTypes.SERVER);
        }
    }

    //This no worky. I'll have to try some other way
    /*public override void OnRoomClientExit()
    { //If the client leaves during the room scene then we should free up their 
        if (Utils.IsSceneActive(GameplayScene))
            return; //Not handling clients leaving the game yet
        if (rolePicker == null)
            rolePicker = GameObject.Find("LobbyCanvas").GetComponent<RolePicker>(); //Fetch the component
        rolePicker.CmdRemoveRoleByID(NetworkClient.localPlayer.netId);
    }*/

    public override void OnGUI()
    {
        base.OnGUI();
    }

}
 