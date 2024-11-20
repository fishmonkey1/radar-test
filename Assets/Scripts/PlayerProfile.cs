using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using Mirror;

[System.Serializable]
public class PlayerProfile : NetworkBehaviour
{
    public string PlayerName = "default";

    public static string LoadedProfileName = null;

    [JsonIgnore] // Ignore during serialization
    public Role CurrentRole = CrewRoles.UnassignedRole;

    public delegate void RoleChangeDelegate(Role oldRole, Role newRole);

    [JsonIgnore]
    public RoleChangeDelegate OnRoleChange;

    [JsonIgnore] // Ignore GameObject references
    private GameObject HorniTank;

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
            bool isLocal = GameObject.ReferenceEquals(gameObject, NetworkClient.localPlayer.gameObject);
            if (isLocal)
            {
                CamCycle.Instance.ChangeRoles(oldRole, role);
                if (HorniTank != null)
                {
                    Debug.Log("Assigning player to spawned tank");
                    if (role.Name == CrewRoles.Gunner.Name)
                        HorniTank.GetComponent<Turret>().SetPlayer(this);
                    if (role.Name == CrewRoles.Driver.Name)
                        HorniTank.GetComponent<tankSteer>().SetPlayer(this);
                }
            }
        }

        if (oldRole == null)
            Debug.Log($"Assigned role named {role.Name} to player");
        else
            Debug.Log($"Changed role from {oldRole.Name} to {role.Name}");
        if (OnRoleChange != null)
        {
            OnRoleChange(oldRole, role);
        }
    }

    /// <summary>
    /// Exports the PlayerName to a JSON file for loading the profile later. This will eventually include experience and unlocks as well, but is simplified for testing/the alpha
    /// </summary>
    /// <param name="directoryPath">The directory to save the profile to. Currently it is Application.persistantDataPath + "/PlayerProfiles"</param>
    public void ExportToJson(string directoryPath)
    {
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
