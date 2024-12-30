using System.Collections.Generic;
using UnityEngine;

public class VehicleSpawnData : MonoBehaviour
{
    List<Role> VehicleRoles = new(); //This tracks all of the roles that this vehicle has, and how many
    List<PlayerProfile> PlayerProfiles = new(); //The profiles that are assigned to this vehicle
    [SerializeField]
    GameObject VehiclePrefab; //The GameObject to instantiate for this vehicle
}
