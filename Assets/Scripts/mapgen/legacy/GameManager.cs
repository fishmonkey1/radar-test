using UnityEngine;

using System.IO;
using System.Collections.Generic;
using UnityEngine.AI;



public class GameManager: MonoBehaviour
{
    [SerializeField] private LayerTerrain layerTerrain;
    [SerializeField] private Biomes biomes;


    public Dictionary<string, int> texturesDict = new Dictionary<string, int>();
    [Header("Enemy Settings")]
    [SerializeField] GameObject GreenEnemyPrefab;
    [SerializeField] GameObject RedEnemyPrefab;
    [SerializeField] GameObject PlayerPrefab;
    [SerializeField] int numOfGreen = 10;
    [SerializeField] int numOfRed = 10;
    public static List<Vector3> enemyLoadPositions = new List<Vector3>();

    public void Awake()
    {
        // if (terrain == null)
        //     terrain = GetComponent<Terrain>(); //Should already be assigned, but nab it otherwise

        Debug.Log("Running GameManager");
        LoadTerrainPrefab(); // create Terrain obj instance

        layerTerrain.layersDict.Add(LayersEnum.Elevation, layerTerrain.elevationLayers);
        layerTerrain.layersDict.Add(LayersEnum.Moisture, layerTerrain.moistureLayers);
        layerTerrain.GenerateTerrain(); // runs all of layerTerrain's stuff

        LoadEnemies(); //spawnb in enemies once terrain is made
        
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

        if (layerTerrain.terrain == null)
        {
            Debug.Log("Unable to create Terrain object.");
        }
        else Debug.Log("Created Terrain Object:   " + layerTerrain.terrain);
        

        
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

        layerTerrain.terrain.terrainData.terrainLayers = layers.ToArray(); //set new layers
        layerTerrain.terrain.terrainData.RefreshPrototypes();
    }

    public void LoadEnemies()
    {
        List<Vector3> points = new List<Vector3>();

        /*while (navmeshPoints.Count < numOfGreen + numOfRed)
        {
            NavMeshHit hit;
            Vector3 randomPoint = new Vector3(Random.Range(0, layerTerrain.X - 1), 1.5f, Random.Range(0, layerTerrain.Y - 1));
            if (NavMesh.SamplePosition(randomPoint, out hit, 25, 1))
            {
                navmeshPoints.Add(hit.position);
            }
        }*/
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

