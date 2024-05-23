using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadLevel : MonoBehaviour
{
    [SerializeField] Transform plane;
    [SerializeField] GameObject GreenEnemyPrefab;
    [SerializeField] GameObject RedEnemyPrefab;
    [SerializeField] GameObject PlayerPrefab;

    [SerializeField] int numOfGreen = 10;
    [SerializeField] int numOfRed = 10;



    public static List<Vector3> enemyLoadPositions = new List<Vector3>();

    void Awake()
    {
        LoadEnemies();
    }

    public void LoadEnemies()
    {
        for (int i = 0; i < numOfGreen; i++)
        {
            Vector3 randomPoint = new Vector3(Random.Range(-20,21), 1, Random.Range(-20, 21));
            var cube=Instantiate(GreenEnemyPrefab, randomPoint, new Quaternion(0, 0, 0, 0));
            //cube.transform.localScale = new Vector3(1, 1, 1);
        }
        for (int i = 0; i < numOfRed; i++)
        {
            Vector3 randomPoint = new Vector3(Random.Range(-20, 21), 1, Random.Range(-20, 21));
            Instantiate(RedEnemyPrefab, randomPoint, new Quaternion(0, 0, 0, 0));
        }
    }
}



