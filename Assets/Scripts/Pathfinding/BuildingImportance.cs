using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;

public static class BuildingImportance
{

    public static List<BuildingIntPair> BuildingValues = new();
    static string path = Application.dataPath + @"\JSON\BuildingImportance.json";

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

    public static BuildingIntPair GetBuildingValue(NodeBuilding building)
    {
        foreach (BuildingIntPair pair in BuildingValues)
        {
            if (pair.Building == building) return pair;
        }
        Debug.LogError($"Asked for building {nameof(building)} value which does not exist");
        return null;
    }

    public static void SaveToJSON()
    {
        var save = new BuildingPairList(BuildingValues);
        var json = JsonConvert.SerializeObject(save, Formatting.Indented);
        File.WriteAllText(path, json);
        Debug.Log("Wrote BuildingImportance.json to disk");
    }

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

public class BuildingPairList
{
    public List<BuildingIntPair> List;

    public BuildingPairList(List<BuildingIntPair> list)
    {
        List = list;
    }
}

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