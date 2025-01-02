using HorniTank;
using System.Collections.Generic;
using UnityEngine;

public class VehicleSpawnData : MonoBehaviour
{
    public List<VehicleData> VehicleData = new();
}

[System.Serializable]
public class VehicleData
{
    public List<RoleScriptableObject> VehicleRoles = new(); //This tracks all of the roles that this vehicle has, and how many
    public List<PlayerProfile> PlayerProfiles = new(); //The profiles that are assigned to this vehicle
    public GameObject VehiclePrefab; //The GameObject to instantiate for this vehicle
}