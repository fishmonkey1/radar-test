using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KaimiraGames;

[System.Serializable]

public class Zone
{
    public string name;

    [Range(1f,99f)]public float minDensityPSD = 1f;
    [Range(2f, 99f)] public float maxDensityPSD = 3f;

    public float elevationMin;
    public float elevationMax;

    public bool IsGrassZone;

    public List<ZoneObject> SpawnedObjects = new List<ZoneObject>();
    public WeightedList<GameObject> weightedObjects = new();
}
