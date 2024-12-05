using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;

/// <summary>
/// JSON file that stores the handwritten weights for buildings in the <see cref="Graph"/>.
/// </summary>
public static class BuildingImportance
{

    public static List<BuildingIntPair> BuildingValues = new();
    static string path = Application.dataPath + @"\JSON\BuildingImportance.json";

    /// <summary>
    /// Import or export from JSON when first being created.
    /// </summary>
    static BuildingImportance()
    {
        //using @ so the string is a literal and there's no attempted escape sequence
        if (File.Exists(path))
        {
            LoadFromJSON();
        }
        else
        {
            AssignDefaults();
            SaveToJSON();
        }
    }

    /// <summary>
    /// Check the weight of a building.
    /// </summary>
    /// <param name="building"></param>
    /// <returns></returns>
    public static BuildingIntPair GetBuildingValue(NodeBuilding building)
    {
        foreach (BuildingIntPair pair in BuildingValues)
        {
            if (pair.Building == building) return pair;
        }
        Debug.LogError($"Asked for building {nameof(building)} value which does not exist");
        return null;
    }

    /// <summary>
    /// Export the BuildingValues list to JSON.
    /// </summary>
    public static void SaveToJSON()
    {
        var save = new BuildingPairList(BuildingValues);
        var json = JsonConvert.SerializeObject(save, Formatting.Indented);
        File.WriteAllText(path, json);
        Debug.Log("Wrote BuildingImportance.json to disk");
    }

    /// <summary>
    /// Import the BuildingValues list from a saved file.
    /// </summary>
    public static void LoadFromJSON()
    {
        if (File.Exists(path))
        {
            var jsonData = File.ReadAllText(path);
            var deserialized = JsonConvert.DeserializeObject<BuildingPairList>(jsonData);

            BuildingValues = deserialized.List;
            Debug.Log("Loaded BuildingImportance.json");
        }
        else
        {
            Debug.LogError("Called BuildingImportance.LoadFromJSON, but no JSON file exists");
        }
    }

    /// <summary>
    /// If there is no file to load, then we use these hardcoded ones for now.
    /// </summary>
    public static void AssignDefaults()
    {
        BuildingValues = new()
           {
            new(NodeBuilding.AMMO_DEPOT, 2),
            new(NodeBuilding.VEHICLE_GARAGE, 2),
            new(NodeBuilding.CHECKPOINT, 3),
            new(NodeBuilding.RADAR, 3),
            new(NodeBuilding.RAIL_DEPOT, 4),
            new(NodeBuilding.VILLAGE_CENTER, 3)
        };
    }

}

/// <summary>
/// Helper class for serializing the BuildingPairLists.
/// </summary>
public class BuildingPairList
{
    public List<BuildingIntPair> List;

    public BuildingPairList(List<BuildingIntPair> list)
    {
        List = list;
    }
}

/// <summary>
/// A NodeBuilding and its paired weight, for saving/loading with our JSON file.
/// </summary>
public class BuildingIntPair
{
    public NodeBuilding Building;
    public int Value;

    public BuildingIntPair() { }

    public BuildingIntPair(NodeBuilding building, int value)
    {
        Building = building;
        Value = value;
    }
}