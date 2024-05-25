using UnityEngine;
using ProcGenTiles;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using UnityEngine.AI;

public class GameManager: MonoBehaviour
{
    [SerializeField] private LayerTerrain layerTerrain;
    [SerializeField] private Biomes biomes;
    [SerializeField] private Terrain terrain;

    public Dictionary<string, int> texturesDict = new Dictionary<string, int>();

    public void Awake()
    {
        if (terrain == null)
            terrain = GetComponent<Terrain>(); //Should already be assigned, but nab it otherwise
      
        layerTerrain.layersDict.Add(LayersEnum.Elevation, layerTerrain.elevationLayers);
        layerTerrain.layersDict.Add(LayersEnum.Moisture, layerTerrain.moistureLayers);
        layerTerrain.GenerateTerrain();

    }

    public void LoadTerrainTextures()
    {
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

