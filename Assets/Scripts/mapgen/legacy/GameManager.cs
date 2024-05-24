using UnityEngine;
using ProcGenTiles;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using UnityEngine.AI;

public class GameManager: MonoBehaviour
{
    [SerializeField]
    private LayerTerrain layerTerrain;

    [SerializeField]
    private Biomes biomes;

/*    private int X;
    private int Y;*/
  
    [SerializeField]
    private Terrain terrain; //This may become a custom mesh in the future, gotta dig up some code on it

    public Dictionary<string, int> texturesDict = new Dictionary<string, int>();


    public void Awake()
    {
        if (terrain == null)
            terrain = GetComponent<Terrain>(); //Should already be assigned, but nab it otherwise

        LoadTextures();

        layerTerrain.layersDict.Add(LayersEnum.Elevation, layerTerrain.elevationLayers);
        layerTerrain.layersDict.Add(LayersEnum.Moisture, layerTerrain.moistureLayers);

        layerTerrain.GenerateTerrain();
        
    }

    public void LoadTextures()
    {
        DirectoryInfo dir = new DirectoryInfo("Assets/Textures_and_Models/Resources/TerrainTextures/png");
        FileInfo[] info = dir.GetFiles("*.png"); //don't get the meta files
        int index = 0;
        List<TerrainLayer> layers = new List<TerrainLayer>();
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
        terrain.terrainData.terrainLayers = layers.ToArray();
    }


/*    public void LoadEnemyBoats()
    {
        // keep trying random points 
        // until we have X (numberOfEnemies) amount of Vector3 points for boats in the enemyBoatLoadPositions list
        while (enemyBoatLoadPositions.Count < numberOfEnemies)
        {
            int randomX = Random.Range(0, X);
            int randomY = Random.Range(0, Y);
            Vector3 randomPoint = new Vector3(randomY, waterHeight, randomX);

            if (layerTerrain.finalMap.GetTile(randomX, randomY).ValuesHere["Land"] == 0)
            {
                enemyBoatLoadPositions.Add(randomPoint);
            }

        }

        for (int i = 0; i < enemyBoatLoadPositions.Count; i++)
        {
            Instantiate(enemyBoat, enemyBoatLoadPositions[i], new Quaternion(0, 0, 0, 0), terrain.transform);
        }
    }*/


}

