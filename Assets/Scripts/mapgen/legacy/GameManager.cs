using UnityEngine;

using System.IO;
using System.Collections.Generic;



public class GameManager: MonoBehaviour
{
    [SerializeField] private LayerTerrain layerTerrain;
    [SerializeField] private Biomes biomes;


    public Terrain terrain;

    public Dictionary<string, int> texturesDict = new Dictionary<string, int>();

    public void Awake()
    {
        // if (terrain == null)
        //     terrain = GetComponent<Terrain>(); //Should already be assigned, but nab it otherwise

        Debug.Log("Running GameManager");
        LoadTerrainPrefab();

        layerTerrain.layersDict.Add(LayersEnum.Elevation, layerTerrain.elevationLayers);
        layerTerrain.layersDict.Add(LayersEnum.Moisture, layerTerrain.moistureLayers);
        layerTerrain.GenerateTerrain();
        
    }

  /*  public void OnDestroy()
    {
        string tl_dir = "Assets/Textures_and_Models/Resources/TerrainTextures/topo/layers/";
        AssetDatabase.DeleteAsset(tl_dir + "/Topographic.terrainlayer");
    }*/

    public void LoadTerrainPrefab()
    {
        Debug.Log("Creating Initial Terrain Object...");
        // Creates new terrain GameObject
        // sets our Terrain for all of our scripts
        // to an instance of this terrain

        TerrainData terrainData = new TerrainData();
        GameObject terrainGO = Terrain.CreateTerrainGameObject(terrainData);
        GameObject terrainInstance = Instantiate(terrainGO);
        layerTerrain.terrain = terrainInstance.GetComponent<Terrain>();

        if (terrain == null)
        {
            Debug.Log("Unable to create Terrain object.");
        }
        else Debug.Log("Created Terrain Object:   " + terrain);
        

        
    }

    public void LoadTerrainTextures()
    {
        Debug.Log("Loading Terrain Textures To Disk");
        DirectoryInfo dir;
        List<TerrainLayer> layers = new List<TerrainLayer>();

        // need to make topo map first and create layer
        layerTerrain.genTopo.createTopoTextures(0, 0, layerTerrain.X, layerTerrain.Y, false);

        if (!layerTerrain.makeTerrainTextureTopo)
        {
            dir = new DirectoryInfo("Assets/Textures_and_Models/Resources/TerrainTextures/png");

            FileInfo[] info = dir.GetFiles("*.png"); //don't get the meta files
            int index = 0;
            
            foreach (FileInfo file in info)
            {
                string fileName = Path.GetFileNameWithoutExtension(file.FullName);

                // Resources.Load() needs a 'Resources' folder, that's where it starts the search.
                string location_from_Resources_folder = "TerrainTextures/layers/";
                TerrainLayer texture = Resources.Load<TerrainLayer>(location_from_Resources_folder + fileName);
                layers.Add(texture);
                texturesDict.Add(fileName, index);
                index++;
            }
        }
        else
        {
            //layerTerrain.genTopo.createTopoTextures(0, 0, layerTerrain.X, layerTerrain.Y, false);
            dir = new DirectoryInfo("Assets/Textures_and_Models/Resources/TerrainTextures/topo/layers/");
            FileInfo[] info = dir.GetFiles("*.terrainlayer"); //don't get the meta files
            int index = 0;
            foreach (FileInfo file in info)
            {   
                string fileName = Path.GetFileNameWithoutExtension(file.FullName);
                Debug.Log("file name: "+fileName);

                // Resources.Load() needs a 'Resources' folder, that's where it starts the search.
                string location_from_Resources_folder = "TerrainTextures/topo/layers/";
                TerrainLayer texture = Resources.Load<TerrainLayer>(location_from_Resources_folder + fileName);
                layers.Add(texture);

                texturesDict.Add(fileName, index);
                index++;
            }
        }
        
        terrain.terrainData.terrainLayers = layers.ToArray(); //set new layers
        terrain.terrainData.RefreshPrototypes();
    }
}

