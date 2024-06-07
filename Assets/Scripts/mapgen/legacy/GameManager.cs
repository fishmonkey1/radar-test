using UnityEngine;

using System.IO;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEditor;



public class GameManager: MonoBehaviour
{
    [SerializeField] private MapGenerator mapGenerator;
    [SerializeField] private RoadGen roadGen;

    [SerializeField] private LayerTerrain layerTerrain;
    //[SerializeField] private Biomes biomes;


    public Dictionary<string, int> texturesDict = new Dictionary<string, int>();
    [Header("Enemy Settings")]
    [SerializeField] public GameObject GreenEnemyPrefab;
    [SerializeField] public GameObject RedEnemyPrefab;
    [SerializeField] GameObject PlayerPrefab;
    [SerializeField] int numOfGreen = 10;
    [SerializeField] int numOfRed = 10;
    public static List<Vector3> enemyLoadPositions = new List<Vector3>();

    public float[,] noiseMap;
    Color[] colorMap;


    public void Awake()
    {
        AssetDatabase.DeleteAsset("Assets/Textures_and_Models/Resources/TerrainTextures/topo/layers/Topographic.terrainlayer");
        AssetDatabase.Refresh();

        loadNewData();
        //LoadEnemies();
    }

    public void loadNewData()
    {
        layerTerrain.GenerateTerrain(); // runs all of layerTerrain's stuff, new noiseMap

        noiseMap = layerTerrain.finalMap.FetchFloatValues(LayersEnum.Elevation);

        layerTerrain.genTopo.createTopoTextures(0, 0, layerTerrain.X, layerTerrain.Y, false, noiseMap);

        if (layerTerrain.drawMode == LayerTerrain.DrawMode.ColorMap)
        {   
            colorMap = mapGenerator.GenerateColorMap(noiseMap);
        }

        RefreshObject();
    }

    public void RefreshObject()
    {
        if (layerTerrain.drawType == LayerTerrain.DrawType.Terrain)
        {
            colorMap = mapGenerator.GenerateColorMap(noiseMap);
            colorMap = roadGen.GetArterialPaths(noiseMap, colorMap);

            Texture2D texture = TextureGenerator.TextureFromColorMap(colorMap, layerTerrain.X, layerTerrain.Y);
            SetTextureOnTerrain(texture);
        }
    }

    private void SetTextureOnTerrain(Texture2D texture)
    {
        TerrainLayer newlayer = new TerrainLayer();
        newlayer.diffuseTexture = texture;
        newlayer.tileSize = new Vector2(layerTerrain.X, layerTerrain.Y); //set tile size so it doesn't tile
        
        TerrainLayer[] newlayerlist = new TerrainLayer[1];
        newlayerlist[0] = newlayer;
        layerTerrain.terrain.terrainData.terrainLayers = newlayerlist;
    }

    public void LoadTerrainTextures()
    {   if (!layerTerrain.DrawInEditor)
        {
            Debug.Log("Loading Terrain Textures To Disk");
            DirectoryInfo dir;
            List<TerrainLayer> layers = new List<TerrainLayer>();

            // need to make topo map first and create layer
            layerTerrain.genTopo.createTopoTextures(0, 0, layerTerrain.X, layerTerrain.Y, false, noiseMap);

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
                    Debug.Log("file name: " + fileName);

                    // Resources.Load() needs a 'Resources' folder, that's where it starts the search.
                    string location_from_Resources_folder = "TerrainTextures/topo/layers/";
                    TerrainLayer texture = Resources.Load<TerrainLayer>(location_from_Resources_folder + fileName);
                    layers.Add(texture);

                    texturesDict.Add(fileName, index);
                    index++;
                }
            }

            layerTerrain.terrain.terrainData.terrainLayers = layers.ToArray(); //set new layers
            layerTerrain.terrain.terrainData.RefreshPrototypes();
        }
    }

    public void LoadEnemies()
    {
        List<Vector3> points = new List<Vector3>();

        Vector3 randomPoint = new Vector3(Random.Range(0, layerTerrain.X - 1), layerTerrain.depth+5, Random.Range(0, layerTerrain.Y - 1));
        int count = 0;
        for (int i = 0; i < numOfGreen; i++)
        {
            Instantiate(GreenEnemyPrefab, randomPoint, new Quaternion(0, 0, 0, 0));
            count += 1;
        }
        for (int i = 0; i < numOfRed; i++)
        {
            Instantiate(RedEnemyPrefab, randomPoint, new Quaternion(0, 0, 0, 0));
            count += 1;
        }
    }

}

