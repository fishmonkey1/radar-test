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

    [SerializeField]
    GameObject canvasPrefab; //Try spawning the prefab instead of leaving it in the scene?

    public static new TankRoomManager singleton => NetworkManager.singleton as TankRoomManager;

    public struct PlayerJoinMessage : NetworkMessage
    {
        public bool hasName;
        public string name;
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);
        Debug.Log("Server added a player");
    }

    public override void OnRoomStartServer()
    {
        
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
        Debug.Log($"Client connected to the server with id of {conn.connectionId}");
    }

    public override void OnRoomServerSceneChanged(string sceneName)
    {
        if (sceneName == RoomScene)
        {
            Debug.Log("Server moved into room scene, spawning canvas");
            GameObject canvas = GameObject.Instantiate(canvasPrefab);
            NetworkServer.Spawn(canvas);
        }
    }

    public override void OnGUI()
    {
        base.OnGUI();
    }

}
 