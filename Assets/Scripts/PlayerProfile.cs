using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using Mirror;
using System;
using System.Collections.Generic;

[System.Serializable]
public class PlayerProfile
{
    /// <summary>
    /// The user's display name in the match.
    /// </summary>
    public string PlayerName = "default";

    /// <summary>
    /// Set by <see cref="NamePicker"/> so the correct profile can be loaded or created by <see cref="TankRoomPlayer"/>
    /// </summary>
    [JsonIgnore, NonSerialized]
    public static string LoadedProfileName = null;

    //Make a struct that holds info about what unlocks they have bought with XP

    //Make a field that handles how much XP they have earned

    //Make a stats portion that tracks how long they've played on the profile, how many kills the tank has gotten while they were in it, pride flags collected

    /// <summary>
    /// Convenient reference to the MonoBehaviour holder for this profile.
    /// </summary>
    [JsonIgnore, NonSerialized]
    public ProfileHolder Holder; //The MonoBehaviour that references this profile

    /// <summary>
    /// The Role that the profile selected with the <see cref="RolePicker"/>. Changing the current role triggers <see cref="OnRoleChange"/>
    /// </summary>
    [JsonIgnore, SerializeField] // Ignore during serialization
    public Role CurrentRole = CrewRoles.UnassignedRole;

    /// <summary>
    /// Definition for the custom <see cref="OnRoleChange"/> delegate.
    /// </summary>
    /// <param name="oldRole">The role this profile had last.</param>
    /// <param name="newRole">The role this profile picked.</param>
    public delegate void RoleChangeDelegate(Role oldRole, Role newRole);

    /// <summary>
    /// Implementation of <see cref="RoleChangeDelegate"/> that control scripts watch.
    /// TODO: This should likely be flagged as an event so only the profile invokes it or deletes all the listeners.
    /// </summary>
   [JsonIgnore, NonSerialized]
    public RoleChangeDelegate OnRoleChange;

    /// <summary>
    /// The HorniTank this profile has a role in. See <see cref="SetHorniTank(GameObject)"/>
    /// </summary>
    [JsonIgnore, NonSerialized] // Ignore GameObject references
    GameObject HorniTank = null;

    /// <summary>
    /// Constructor assigns the static name to itself and subscribes to <see cref="RoomNetworking.OnChangeHorniTankEvent"/>
    /// </summary>
    public PlayerProfile() 
    {
        PlayerName = LoadedProfileName;
        if (TankRoomManager.singleton != null)
        { //Okay. Let's try this instead so it doesn't try to run in the editor
            if (TankRoomManager.singleton.RoomNetworking != null)
                TankRoomManager.singleton.RoomNetworking.OnChangeHorniTankEvent += SetHorniTank;
        }
    }

    /// <summary>
    /// Called by the <see cref="TankRoomManager"/> when there is a tank spawned for this profile.
    /// </summary>
    /// <param name="horniTank">The tank spawned by the <see cref="TankRoomManager"/></param>
    void SetHorniTank(GameObject horniTank)
    {
        if (HorniTank == null)
        { //A tank hasn't been assigned, so let's update our reference
            HorniTank = horniTank; //And that's pretty much it
            //Attempt setting up the camera now that there's a tank to use
            Debug.Log("HorniTank was set via a SyncVar hook. Attempting to set up cameras");
            SelectRole(CurrentRole);
        }
        //Otherwise we'll ignore this for now, since I only have one tank. TODO: Support other vehicles
    }

    /// <summary>
    /// Called from the RolePicker script, this function assigns the role and runs cleanup on the old role. If the gameplay scene is up, this will also inform the CamCycle script and the relevant control script as well. In the lobby, there is no HorniTank spawned, so info is carried over when the lobby ends and the game begins.
    /// </summary>
    /// <param name="role">The new role to be assigned.</param>
    public void SelectRole(Role role)
    {
        //This will later need checks to make sure the picked role isn't over the limit
        Role oldRole = CurrentRole;
        CurrentRole = role;
        if (Utils.IsSceneActive(TankRoomManager.singleton.GameplayScene))
        {
            if (Holder == null)
            { //I don't know why I'm getting a NullReferenceError here, so I'm gonna try this :c
                Holder = NetworkClient.localPlayer.GetComponent<ProfileHolder>();
                Holder.Profile = this;
            }
            bool isLocal = Holder.IsLocalPlayer(); //Check if we're on the player's owned profile before doing CamCycle
            Debug.Log($"Profile is in the game scene. isLocal is set to {isLocal}. Current role is {CurrentRole.Name} and profile name is {PlayerName}");
            if (isLocal)
            {
                Debug.Log("Profile is local, setting up cameras and control scripts. HorniTank value is " + HorniTank);
                CamCycle.Instance.ChangeRoles(oldRole, role);
                if (HorniTank != null)
                { //We can't set up our roles if a tank hasn't been spawned
                    Debug.Log("Assigning player to spawned tank");
                    if (role == CrewRoles.Gunner)
                        HorniTank.GetComponent<Turret>().SetPlayer(this);
                    if (role == CrewRoles.Driver)
                        HorniTank.GetComponent<tankSteer>().SetPlayer(this);
                    if (role == CrewRoles.Radar)
                        HorniTank.GetComponent<RadarRole>().SetPlayer(this);
                }
            }
        }

        if (oldRole == null || oldRole == CrewRoles.UnassignedRole)
            Debug.Log($"Assigned role named {role.Name} to player");
        else
            Debug.Log($"Changed role from {oldRole.Name} to {role.Name}");
        OnRoleChange?.Invoke(oldRole, role);
    }

    /// <summary>
    /// Exports the PlayerName to a JSON file for loading the profile later. This will eventually include experience and unlocks as well, but is simplified for testing/the alpha
    /// </summary>
    /// <param name="directoryPath">The directory to save the profile to. Currently it is Application.persistantDataPath + "/PlayerProfiles"</param>
    public void ExportToJson(string directoryPath)
    {
        this.PlayerName = LoadedProfileName;
        string json = JsonConvert.SerializeObject(this, Formatting.Indented);
        string filePath = Path.Combine(directoryPath, $"{PlayerName}.json");

        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        File.WriteAllText(filePath, json);
        Debug.Log($"PlayerProfile saved to {filePath}");
    }

    /// <summary>
    /// Imports a player profile with a given filename. It returns the loaded profile if it was found, otherwise sending back null.
    /// </summary>
    /// <param name="filePath">The filepath to locate the profile in. Currently it is Application.persistentDataPath + "/PlayerProfiles"</param>
    /// <returns></returns>
    public static PlayerProfile ImportFromJson(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError($"File not found: {filePath}");
            return null;
        }

        string json = File.ReadAllText(filePath);
        PlayerProfile profile = JsonConvert.DeserializeObject<PlayerProfile>(json);
        Debug.Log($"PlayerProfile loaded from {filePath}");
        return profile;
    }

    /// <summary>
    /// Attempt to load a profile from the disk, and if found places it in the profile parameter. Returns true if it loaded a profile, and returns false if it did not.
    /// </summary>
    /// <param name="directoryPath">The directory to locate the profile in. Currently it is Application.persistentDataPath + "/PlayerProfiles"</param>
    /// <param name="playerName">The name of the player, which is also the filename of the profile.</param>
    /// <param name="profile">If a profile is found then it is loaded into this variable. This is null if no profile is found.</param>
    /// <returns>Boolean indicating if a profile was found.</returns>
    public static bool TryLoadProfile(string directoryPath, string playerName, out PlayerProfile profile)
    {
        string filePath = Path.Combine(directoryPath, $"{playerName}.json");

        if (File.Exists(filePath))
        {
            profile = ImportFromJson(filePath);
            return true;
        }

        profile = null;
        return false;
    }
}

/// <summary>
/// Convenience class for linking a group of profiles to a tank for when multiple vehicles are implemented.
/// </summary>
public class ProfileGroup
{
    public List<PlayerProfile> Group = new();
}
