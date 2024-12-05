using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using UnityEngine;

/// <summary>
/// Stub for later when enemies need a way to size up a player and determine how difficult an encounter they should try.
/// </summary>
public static class ThreatMap
{

    /// <summary>
    /// This maps each enemy type to a level of danger for helping the enemies understand what they need to handle the player
    /// Also helps with determining when they should retreat and how much they'll need to reinforce
    /// These numbers will definitely need tweaking, so it may be wise to do these in a JSON file later
    /// </summary>
    public static Dictionary<EnemyType, float> EnemyThreatMap = new()
    {
        { EnemyType.INFANTRY, 0.1f},
        { EnemyType.TRANSPORT_TRUCK, 0.25f },
        { EnemyType.APC, 0.5f },
        { EnemyType.BTR, 0.75f },
        { EnemyType.ROCKET_TRUCK, 0.8f },
        { EnemyType.RADAR_TRUCK, 0.05f },
        { EnemyType.LIGHT_TANK, 0.9f },
        { EnemyType.HEAVY_TANK, 1f },
        { EnemyType.HELICOPTER, 1.5f },
        { EnemyType.JET, 2f }
    };

    /// <summary>
    /// Import the map from a saved JSON file.
    /// </summary>
    /// <param name="filePath"></param>
    public static void LoadThreatMapFromJson(string filePath)
    {
        try
        {
            string json = File.ReadAllText(filePath);
            if (EnemyThreatMap.Count != 0)
            { //If there are entries, we should clear the dictionary
                EnemyThreatMap.Clear();
            }
            // Deserialize the JSON to a string, float dictionary for parsing
            Dictionary<string, float> tempDangerMap = JsonConvert.DeserializeObject<Dictionary<string, float>>(json);

            bool loadSuccess = true;
            foreach (var entry in tempDangerMap)
            {
                if (System.Enum.TryParse(entry.Key, out EnemyType type))
                    EnemyThreatMap.Add(type, entry.Value);
                else
                {
                    Debug.LogWarning($"Failed to parse key {entry.Key} into EnemyType enum");
                    loadSuccess = false;
                }
            }
            if (loadSuccess)
                Debug.Log("EnemyThreatMap successfully loaded");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load EnemyThreatMap from JSON with error: {e.Message}");
        }
    }

    /// <summary>
    /// Export the map to a JSON file.
    /// </summary>
    /// <param name="filePath"></param>
    public static void SaveThreatMapToJson(string filePath)
    {
        if (EnemyThreatMap.Count == 0)
        {
            Debug.LogWarning("There isn't anything in the threat map to serialize! Aborting...");
            return;
        }
        //Convert the current dictionary into a new <string, float> dictionary for serializing

        Dictionary<string, float> serializeDict = new();

        foreach (var entry in EnemyThreatMap)
        {
            string key = entry.Key.ToString(); //Convert the enum type into the all caps name
            serializeDict.Add(key, entry.Value);
        }

        string json = JsonConvert.SerializeObject(serializeDict, Formatting.Indented); //Make it pretty like Lexi :3
        File.WriteAllText(filePath, json); //Creates file if it doesnt exist, or overwrites the contents if it does
        Debug.Log($"Successfully saved EnemyThreatMap to {filePath}");
    }

}
