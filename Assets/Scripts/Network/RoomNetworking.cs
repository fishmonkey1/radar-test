using UnityEngine;
using Mirror;
using System.Collections.Generic;

/// <summary>
/// Since the TankRoomManager can't send messages, this is my workaround so room messages can be passed around.
/// </summary>
public class RoomNetworking : NetworkBehaviour
{

    [SyncVar(hook = nameof(SetHorniTank))]
    public GameObject HorniTank = null; //When our one and only tank gets spawned, we assign it here.
    TankRoomManager roomManager;

    //Implement delegate as an event for encapsulation. This prevents subscribers from clearing the delegate or invoking it themselves
    /// <summary>
    /// Event is invoked by the SyncVar hook SetHorniTank. Using an event to encapsulate the field and allow only subscribe/unsubscribe on other scripts.
    /// </summary>
    public event OnChangeHorniTank OnChangeHorniTankEvent;
    public delegate void OnChangeHorniTank(GameObject HorniTank); //Delegate to fire when we change the tank reference

    public event OnSpawnPlayerVehicle OnSpawnPlayerVehicleEvent;
    public delegate void OnSpawnPlayerVehicle(GameObject SpawnedVehicle);

    void Awake()
    {
        roomManager = TankRoomManager.singleton; //In case we need to call things over there or set stuff
    }

    void SetHorniTank(GameObject oldTank, GameObject newTank)
    {
        Debug.Log("HorniTank has been updated, SyncVar hook called on client");
        OnChangeHorniTankEvent?.Invoke(newTank);
    }

    [ClientRpc]
    public void RpcSpawnVehicle(GameObject spawnedVehicle, ProfileGroup crew)
    {
        //Server made a vehicle, now the players need to be informed of it
        PlayerProfile local = TankRoomManager.LocalPlayerProfile;
        foreach (PlayerProfile profile in crew.Group)
        {
            if (profile.PlayerName == local.PlayerName)
            { //This player signed up to be on this tank, so let's set them up with their role
                profile.SetHorniTank(spawnedVehicle);
            }
        }
    }

    [ClientRpc]
    public void RpcBroadcastVehicleSpawnData(VehicleData[] vehicles, ProfileGroup[] groups)
    {
        //Match up the vehicles to the groups by index, since it was sent from a dictionary
        //TODO: verify the dictionary is sent properly
        Dictionary<VehicleData, ProfileGroup> data = new();
        for (int i = 0; i < vehicles.Length; i++)
        {
            VehicleData vehicle = vehicles[i];
            ProfileGroup group = groups[i];
            data.Add(vehicle, group);
        }
        TankRoomManager.singleton.GroupToVehicles = data; //Set the local TankRoomManager's dictionary with the sent data
    }

}
