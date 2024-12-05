using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// TODO: Confirm if this script is still needed. Looks like a leftover import, but I should check with Lexi.
/// </summary>
public class LoadLevel : MonoBehaviour
{   
    [Header("Terrain Settings")]
    [SerializeField] LayerTerrain layerTerrain;

    [Header("Enemy Settings")]
    [SerializeField] private LayerMask ground;
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
            Vector3 randomPoint = new Vector3(Random.Range(0, layerTerrain.X - 1), layerTerrain.depth + 5, Random.Range(0, layerTerrain.Y - 1));
            //hit = Physics.Raycast(randomPoint, new Vector3(0,-1,0), layerTerrain.depth + 5);
            
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



