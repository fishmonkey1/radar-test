using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class TankRoomManager : NetworkRoomManager
{
    /// Subclass the NetworkRoomManager so it takes into account the roles that are able to be picked
    /// Don't let players ready up unless they have a role picked
    /// Figure out how I'm linking up roles with their associated scripts at some point :c

    [SerializeField] GameObject canvasPrefab; //Try spawning the prefab instead of leaving it in the scene?
    [SerializeField] GameObject horniTankPrefab; //For spawning after the game scene is loaded
    public GameObject horniTank; //The spawned tank for grabbing components off of

    public static new TankRoomManager singleton => NetworkManager.singleton as TankRoomManager;

    public override void ReadyStatusChanged()
    {
        base.ReadyStatusChanged();
        Debug.Log("Player is now ready");
    }

    public override GameObject OnRoomServerCreateGamePlayer(NetworkConnectionToClient conn, GameObject roomPlayer)
    {
        // get start position from base class
        Transform startPos = GetStartPosition();
        GameObject gamePlayer = startPos != null
            ? Instantiate(playerPrefab, startPos.position, startPos.rotation)
            : Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);

        PlayerInfo newInfo = gamePlayer.GetComponent<PlayerInfo>();
        TankRoomPlayer roomInfo = roomPlayer.GetComponent<TankRoomPlayer>();

        newInfo.PickRole(CrewRoles.GetRoleByID(roomInfo.RoleID)); //Set the role to what was in the room player
        newInfo.PickName(roomInfo.PlayerName);

        return gamePlayer; //Send the player prefab back
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);
        Debug.Log("Server added a player with a netId of " + conn.identity.netId);
    }

    public override void OnRoomStopClient()
    {
        base.OnRoomStopClient();
    }

    public override void OnRoomStopServer()
    {
        base.OnRoomStopServer();
    }

    public override void OnRoomServerConnect(NetworkConnectionToClient conn)
    {
        Debug.Log($"Client connected to the server with server connection id of {conn.connectionId}");
    }

    public override void OnRoomServerSceneChanged(string sceneName)
    {
        if (sceneName == RoomScene)
        {
            Debug.Log("Server moved into room scene, spawning canvas");
            GameObject canvas = GameObject.Instantiate(canvasPrefab);
            NetworkServer.Spawn(canvas);
        }
        if (sceneName == GameplayScene)
        {
            //Time to spawn the tank in
            //We'll worry about picking a proper spawn point later on
            Debug.Log("Server moved into gameplay scene, spawning tank");
            horniTank = GameObject.Instantiate(horniTankPrefab); //Double check this puts the tank at 0,0,0
            NetworkServer.Spawn(horniTank);
        }
    }

    public override void OnGUI()
    {
        base.OnGUI();
    }

}
 