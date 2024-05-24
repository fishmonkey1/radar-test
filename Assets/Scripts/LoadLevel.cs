using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using ProcGenTiles;

public class LoadLevel : MonoBehaviour
{   
    [Header("Terrain Settings")]
    [SerializeField] LayerTerrain layerTerrain;

    [Header("Enemy Settings")]
    [SerializeField] GameObject GreenEnemyPrefab;
    [SerializeField] GameObject RedEnemyPrefab;
    [SerializeField] GameObject PlayerPrefab;
    [SerializeField] int numOfGreen = 10;
    [SerializeField] int numOfRed = 10;


    public static List<Vector3> enemyLoadPositions = new List<Vector3>();

    void Start()
    {
        LoadEnemies();
    }


    public void LoadEnemies()
    {
        for (int i = 0; i < numOfGreen; i++)
        {
            Vector3 randomPoint = new Vector3(Random.Range(0, layerTerrain.X-1), 1, Random.Range(0, layerTerrain.Y-1));
            Instantiate(GreenEnemyPrefab, randomPoint, new Quaternion(0, 0, 0, 0));
        }
        for (int i = 0; i < numOfRed; i++)
        {
            Vector3 randomPoint = new Vector3(Random.Range(0, layerTerrain.X), 1, Random.Range(0, layerTerrain.Y));
            Instantiate(RedEnemyPrefab, randomPoint, new Quaternion(0, 0, 0, 0));
        }
    }
}



