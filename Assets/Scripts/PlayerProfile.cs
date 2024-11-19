using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using Mirror;

[System.Serializable]
public class PlayerProfile : NetworkBehaviour
{
    public string PlayerName = "default";

    public static string LoadedProfile = null;

    [JsonIgnore] // Ignore during serialization
    public Role CurrentRole = CrewRoles.UnassignedRole;

    public delegate void RoleChangeDelegate(Role oldRole, Role newRole);

    [JsonIgnore]
    public RoleChangeDelegate OnRoleChange;

    [JsonIgnore] // Ignore GameObject references
    private GameObject HorniTank;

    // Export the PlayerProfile to a JSON file
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

    // Import a PlayerProfile from a JSON file
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
}
