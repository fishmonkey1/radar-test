using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class Flora : MonoBehaviour
{
    [SerializeField] List<GameObject> floraPrefabs = new List<GameObject>();

    [SerializeField] float radius = 2;
    [SerializeField] Vector2 sampleRegionSize = Vector2.one;
    [SerializeField] int numSamplesBeforeRejection = 30;

    List<Vector2> points = new List<Vector2>();

    private void Awake()
    {
        DestroyExisting();
    }
    private void Start()
    {
        points = PoissonDiscSampling.GeneratePoints(radius, sampleRegionSize, numSamplesBeforeRejection);
        Debug.Log(points.Count);
        foreach (Vector2 point in points)
        {
            int index = Random.Range(0, floraPrefabs.Count);
            GameObject tree = Instantiate(floraPrefabs[index], new Vector3(point.x, 0, point.y) + transform.position, Quaternion.identity) as GameObject;
            tree.transform.Rotate(0, Random.Range(0, 360), 0);
            //tree.transform.localScale = Vector3.one * display_radius;
        }
    }

    private void DestroyExisting()
    {
        // destroy existing
        GameObject[] clones = GameObject.FindGameObjectsWithTag("Flora");
        foreach (var clone in clones)
        {
            Destroy(clone);
        }
    }

    /*private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(sampleRegionSize / 2, sampleRegionSize);
        if (points != null)
        {
            foreach (Vector2 point in points)
            {
                Gizmos.DrawSphere(new Vector3(point.x, 0, point.y) + transform.position, radius);
            }
        }
    }*/





}
