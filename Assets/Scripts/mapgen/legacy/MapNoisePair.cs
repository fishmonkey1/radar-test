using ProcGenTiles;
using UnityEngine;

[System.Serializable]
public class MapNoisePair
{
    public Map Map;
    public NoiseParams NoiseParams;

    // TODO: remove! No longer needed
    [HideInInspector] public TextAsset JSON;
    [HideInInspector] public bool UseJsonFile;
    
    public bool LoadFromJSON;
    public bool SaveToJSON;

    public MapNoisePair(Map map, NoiseParams noiseParams)
    {
        this.Map = map;
        this.NoiseParams = noiseParams;
    }
}
