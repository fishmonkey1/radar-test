using UnityEngine;
using ProcGenTiles;
using System.IO;
using System.Collections.Generic;


public class LayerTerrain : MonoBehaviour
{
    public TerrainSize terrainSize = TerrainSize._256;

    [SerializeField] public MeshRenderer MeshRenderer;
    [SerializeField] public MeshFilter MeshFilter;

    [HideInInspector] public int X;
    [HideInInspector] public int Y;
    private bool mesh_32bit_buffer = false;

    [SerializeField] [Range(5, 100)] public int depth; //Maybe rename to height instead? depth is kinda lame

    private float noiseScale; //For transforming the int coords into smaller float values to sample the noise better. Functions as zoom in effect

    //Assign layers from the inspector. In the future I either want ScriptableObjects that can be dragged in or JSON serialization so these don't get lost on a reset
    [SerializeField] public MapLayers elevationLayers;
    [SerializeField] public MapLayers moistureLayers;

    [SerializeField] private FastNoiseLite noise;


    public Map finalMap { get; private set; } //This is where all of the layers get combined into.
    public Pathfinding pathfinding;

    public Dictionary<string, MapLayers> layersDict = new Dictionary<string, MapLayers>();

    [HideInInspector] public float highest_e = -100;
    [HideInInspector] private float lowest_e = 100;

    [Header("Editor ")]
    public bool DrawInEditor;
    public bool autoUpdate;

    [Header("Topo Map Stuff")]
    [SerializeField] public CreateTopoMap genTopo;
    [SerializeField] public GameObject topoObject;
    [SerializeField] public int numberOfTopoLevels;
    [SerializeField] public Color topoColor1;
    [SerializeField] public Color topoColor2;
    [SerializeField] public bool makeTerrainTextureTopo = true;
    // ----------------- DEBUG STUFF
    bool print_debug = false;


    public enum TerrainSize
    {   
        // if you add more, remember to add in SetTerrainSize() too :3
        _8192,
        _4096,
        _2048,
        _1024,
        _512,
        _256,
        _128,
        _64,
        _32,
    }



    public void Awake()
    {
        layersDict.Add(LayersEnum.Elevation, elevationLayers);
        layersDict.Add(LayersEnum.Moisture, moistureLayers);
    }
    public void Start()
    {
        //if (terrain == null) Debug.Log("layerTerrain has no terrain obj");//Should already be assigned, but nab it otherwise
    }


    public void SetTerrainSize()
    {   
        
        if (terrainSize == TerrainSize._8192) { X = 8192; Y = 8192; mesh_32bit_buffer = true; };
        if (terrainSize == TerrainSize._4096) { X = 4096; Y = 4096; mesh_32bit_buffer = true; };
        if (terrainSize == TerrainSize._2048) { X = 2048; Y = 2048; mesh_32bit_buffer = true; };
        if (terrainSize == TerrainSize._1024) { X = 1024; Y = 1024; mesh_32bit_buffer = true; };
        if (terrainSize == TerrainSize._512)  { X = 512;  Y = 512;  mesh_32bit_buffer = true; };
        if (terrainSize == TerrainSize._256)  { X = 256;  Y = 256; };
        if (terrainSize == TerrainSize._128)  { X = 128;  Y = 128; };
        if (terrainSize == TerrainSize._64)   { X = 64;   Y = 64; };
        if (terrainSize == TerrainSize._32)   { X = 32;   Y = 32; };
    }

    public void GenerateTerrain() //main entry
    {
        SetTerrainSize();

        finalMap = new Map(X, Y); //Change this to only create a new map if the sizes differ. It might be getting garbe collected each time, and there's no reason
        pathfinding = new Pathfinding(finalMap); //Init the pathfinding for adjusting regions after they're created

        for (int i = 0; i < elevationLayers.NoisePairs.Count; i++)
        {
            MapNoisePair pair = elevationLayers.NoisePairs[i];
            if (pair.UseJsonFile)
            {
                pair.NoiseParams = JsonUtility.FromJson<NoiseParams>(pair.JSON.text);
            }
            ReadNoiseParams(pair.NoiseParams); //Feed the generator this layer's info
            GenerateHeightmap(pair, LayersEnum.Elevation); //This function handles adding the layer into the finalMap, but it's not very clear. Needs cleaning up to be more readable                                    
        }

        NormalizeFinalMap(LayersEnum.Elevation, elevationLayers.NoisePairs[0].NoiseParams.minValue, elevationLayers.NoisePairs[0].NoiseParams.raisedPower); //Make the final map only span from 0 to 1

        CreateMeshFromHeightmap(); 
    }


    public void CreateMeshFromHeightmap()
    {
        float[,] fmap = finalMap.FetchFloatValues(LayersEnum.Elevation);

        MeshData meshData = MeshGenerator.GenerateTerrainMesh(fmap, depth);
        Mesh mesh = meshData.CreateMesh(mesh_32bit_buffer);
        
        MeshFilter.sharedMesh = mesh;
    } 


    




    public void ReadNoiseParams(NoiseParams noiseParams) //STAYS
    {
        //Read the noise info from the MapLayer and set all of the FastNoiseLite fields here
        if (noise == null)
            noise = new FastNoiseLite();

        noiseScale = noiseParams.noiseScale;

        noise.SetSeed(noiseParams.seed);
        noise.SetNoiseType(noiseParams.noiseType);
        noise.SetFractalType(noiseParams.fractalType);
        noise.SetFractalGain(noiseParams.fractalGain);
        noise.SetFractalLacunarity(noiseParams.fractalLacunarity);
        noise.SetFractalOctaves(noiseParams.fractalOctaves);
        noise.SetFrequency(noiseParams.frequency);
        noise.SetFractalWeightedStrength(noiseParams.weightedStrength);
    }


    public void GenerateHeightmap(MapNoisePair noisePair, string layer) //STAYS
    {
        highest_e = -100;
        lowest_e = 100;
        noisePair.Map = new Map(X, Y); //The map isn't being generated in the inspector, so it must be created here
        for (int y = 0; y < Y; y++)
        {
            for (int x = 0; x < X; x++)
            { //Inner for loop does most of the heavy lifting
                Tile tile = noisePair.Map.Tiles[x, y]; //Get the tile at the location

                float noiseValue = noise.GetNoise(x * noiseScale, y * noiseScale); //Grab the value between -1 and 1
                noiseValue = Mathf.InverseLerp(-1, 1, noiseValue); //set to 0  and 1 scale

                //Set the elevation to the normalized value by checking if we've already set elevation data
                if (tile.ValuesHere.ContainsKey(layer))
                    tile.ValuesHere[layer] = noiseValue;
                else
                    tile.ValuesHere.Add(layer, noiseValue);

                Tile finalTile = finalMap.GetTile(x, y);

                if (finalTile.ValuesHere.ContainsKey(layer))
                { //If the value exist increment the final tile by the amount of the noise
                    finalTile.ValuesHere[layer] += noiseValue;
                }
                else
                { //Otherwise we add it with the value from the first layer
                    finalTile.ValuesHere.Add(layer, noiseValue); //Create the entry and assign the first layer's value
                }

                //track highest lowest values for later
                if (finalTile.ValuesHere[layer] < lowest_e) { lowest_e = finalTile.ValuesHere[layer]; };
                if (finalTile.ValuesHere[layer] > highest_e) { highest_e = finalTile.ValuesHere[layer]; };
            }
        }
    }

    public void NormalizeFinalMap(string layer, float minValue, float raisedPower) //STAYS
    {
        //float range = layersDict[layer].SumOfNoiseLayers();
        float lowest = 100;
        float highest = -100;

        float lowest_after = 100;
        float highest_after = -100;

        for (int y = 0; y < Y; y++)
        {
            for (int x = 0; x < X; x++)
            {
                Tile finalTile = finalMap.GetTile(x, y);

                // just for debug
                if (finalTile.ValuesHere[layer] < lowest) lowest = finalTile.ValuesHere[layer];
                if (finalTile.ValuesHere[layer] > highest) highest = finalTile.ValuesHere[layer];

                finalTile.ValuesHere[layer] = Mathf.InverseLerp(lowest_e, highest_e, finalTile.ValuesHere[layer]);
                if (layer == LayersEnum.Elevation)
                {
                    // TODO: do power stuff here not in GenTerrain()
                    finalTile.ValuesHere[layer] = Mathf.Pow(finalTile.ValuesHere[layer], raisedPower);

                    // TODO: do min-height stuff here, set anything below water level texture to water level.
                    //if (finalTile.ValuesHere[layer] < minValue) finalTile.ValuesHere[layer] = minValue;
                    // commented out becuz URP
                    //finalTile.ValuesHere[layer] = Mathf.Max(0, finalTile.ValuesHere[layer] - minValue);
                }

                // just for debug
                if (finalTile.ValuesHere[layer] < lowest_after) lowest_after = finalTile.ValuesHere[layer];
                if (finalTile.ValuesHere[layer] > highest_after) highest_after = finalTile.ValuesHere[layer];
            }
        }



        if (print_debug)
        {
            Debug.Log($"Lowest value before normalizing was {lowest} and highest was {highest} on {layer} layer ");
            Debug.Log($"Lowest value after normalizing was {lowest_after} and highest was {highest_after} on {layer} layer ");
        }
    }

    public void UpdateTerrainHeightmap(int xBase, int yBase, float[,] heightmap) //MOVE?
    { // TODO: This might need work to instead mark the terrain as dirty until all deform operations are done, and THEN we set the heights
       // terrain.terrainData.SetHeights(xBase, yBase, heightmap); //Fuck you SetHeights, why do you pretend like I can update regions with the xBase and yBase when you actually suck?

        //Because fuck you, that's why! >:)
    }

    public void SerializeNoiseParamsToJson() //MOVE
    { //For each NoiseParam in our layers we serialize them with the naming convention of layer + index in list
        for (int i = 0; i < elevationLayers.NoisePairs.Count; i++)
        {
            MapNoisePair pair = elevationLayers.NoisePairs[i];
            string json = pair.NoiseParams.SerializeParamsToJson(); // Get the JSON string

            // Define the path for the JSON file in the JSON folder inside the Assets folder
            string folderPath = Path.Combine(Application.dataPath, "JSON");
            string filePath = Path.Combine(folderPath, $"layer{i}.json");

            // Create the JSON folder if it doesn't exist
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // Write the JSON string to the file
            File.WriteAllText(filePath, json);

            Debug.Log($"JSON file saved to: {filePath}");
            SerializeMapToJson();
        }
    }

    public void SerializeMapToJson()
    {
        //LayerTerrain lt = GetComponent<LayerTerrain>();
        string json = JsonUtility.ToJson(this, true);
        Debug.Log(json);

        string folderPath = Path.Combine(Application.dataPath, "JSON");
        string filePath = Path.Combine(folderPath, $"wholemap.json");

        // Create the JSON folder if it doesn't exist
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        // Write the JSON string to the file
        File.WriteAllText(filePath, json);
        Debug.Log($"wholemap JSON file saved to: {filePath}");
    }

    public void LoadNoiseParamsFromJson() //MOVE
    {
        for (int i = 0; i < elevationLayers.NoisePairs.Count; i++)
        {
            MapNoisePair pair = elevationLayers.NoisePairs[i];
            if (pair.UseJsonFile && pair.JSON.text != string.Empty)
            { //Only load if the bool is on and the json asset is assigned
                pair.NoiseParams = JsonUtility.FromJson<NoiseParams>(pair.JSON.text);
            }
        }
    }

    public void runMapGen()
    {
        GenerateTerrain();
    }

}
