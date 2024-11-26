using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

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

    void Awake()
    {
        roomManager = TankRoomManager.singleton; //In case we need to call things over there or set stuff
    }

    void SetHorniTank(GameObject oldTank, GameObject newTank)
    {
        Debug.Log("HorniTank has been updated, SyncVar hook called on client");
        OnChangeHorniTankEvent?.Invoke(newTank);
    }

}
