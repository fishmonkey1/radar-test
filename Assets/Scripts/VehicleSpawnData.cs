using HorniTank;
using System;
using System.Collections.Generic;
using UnityEngine;

public class VehicleSpawnData : MonoBehaviour
{
    public List<VehicleData> VehicleData = new();
}

[System.Serializable]
public class VehicleData
{
    /// <summary>
    /// How many roles this vehicle has. Also serves as the max players the vehicle can have.
    /// </summary>
    public List<RoleScriptableObject> VehicleRoles = new();
    /// <summary>
    /// Return the maximum number of players 
    /// </summary>
    public uint MaxPlayers { get
        {
            uint MaxPlayers = 0;
            foreach (RoleScriptableObject role in VehicleRoles)
            {
                MaxPlayers += role.PlayerLimit;
            }
            return MaxPlayers;
            }
        private set { }
        }
    /// <summary>
    /// The players that are crewing this vehicle
    /// </summary>
    public List<PlayerProfile> PlayerProfiles = new(); //The profiles that are assigned to this vehicle
    /// <summary>
    /// The linked object to wire up when spawned
    /// </summary>
    [NonSerialized]
    public GameObject VehiclePrefab; //The GameObject to instantiate for this vehicle
    /// <summary>
    /// The ID of this vehicle prefab.
    /// </summary>
    public uint VehicleId;
    /// <summary>
    /// The display name of the vehicle
    /// </summary>
    public string VehicleName;
}