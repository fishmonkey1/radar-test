using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using ProcGenTiles;
using UnityEngine.AI;

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

    public void LoadEnemies()
    {
        List<Vector3> navmeshPoints = new List<Vector3>();

        while (navmeshPoints.Count < numOfGreen + numOfRed)
        {
            NavMeshHit hit;
            Vector3 randomPoint = new Vector3(Random.Range(0, layerTerrain.X - 1), 1.5f, Random.Range(0, layerTerrain.Y - 1));
            if (NavMesh.SamplePosition(randomPoint, out hit, 25, 1))
            {
                navmeshPoints.Add(hit.position);
            }
        }

        int count = 0;
        for (int i = 0; i < numOfGreen; i++)
        {
            Instantiate(GreenEnemyPrefab, navmeshPoints[count], new Quaternion(0, 0, 0, 0));
            count += 1;
        }
        for (int i = 0; i < numOfRed; i++)
        {
            Instantiate(RedEnemyPrefab, navmeshPoints[count], new Quaternion(0, 0, 0, 0));
            count += 1;
        }
    }
}



