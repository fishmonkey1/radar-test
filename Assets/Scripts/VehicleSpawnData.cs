using HorniTank;
using System;
using System.Collections.Generic;
using UnityEngine;

public class VehicleSpawnData : MonoBehaviour
{
    public List<VehicleDataAndPrefab> VehicleDataAndPrefab = new();
}

/// <summary>
/// Linking together the prefab to spawn with the network information about the tank.
/// </summary>
[Serializable]
public class VehicleDataAndPrefab
{
    public GameObject prefab; //Spawn this on the server and replicate to clients
    public VehicleData VehicleData;
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
    /// The ID of this vehicle prefab.
    /// </summary>
    public uint VehicleId;
    /// <summary>
    /// The display name of the vehicle
    /// </summary>
    public string VehicleName;
}