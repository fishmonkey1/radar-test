using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using ProcGenTiles;

public class Flora : MonoBehaviour
{
    [SerializeField] List<GameObject> floraPrefabs = new List<GameObject>();

    [SerializeField] List<Zone> Zones = new List<Zone>();

    [SerializeField] [Range(4f, 30f)] int minRadius = 6;
    [SerializeField] [Range(4f, 30f)] int maxRadius = 20;
    [SerializeField] Vector2 sampleRegionSize = Vector2.one;
    [SerializeField] int numSamplesBeforeRejection = 30;
    [SerializeField] float floraPrefabsScaleMin;
    [SerializeField] float floraPrefabsScaleMax;

    List<Vector2> points = new List<Vector2>();
    List<Vector3> gizmo = new List<Vector3>();

    LayerTerrain lt;
    public Map biomeMap;
    public float[,] meshHeights;

    public bool EditorAutoUpdate = true;

    // highest() gets highest Y val on mesh, just going to use this to do quick and dirty elevation maffs for the spawner
    [SerializeField] GameObject terrainPlaneObj;
    float highestPoint = 50;

    private void Awake()
    {
        DestroyExisting();   
    }
    private void Start()
    { 
        GenPSD();
    }

   

    public void GenPSD()
    {
        //highest();
        points = PoissonDiscSampling.GeneratePoints(minRadius, maxRadius, sampleRegionSize, numSamplesBeforeRejection);
        
        // do a raycast down to find surface, then select random prefab and place
        foreach (Vector2 point in points)
        {
            int index = Random.Range(0, floraPrefabs.Count); // select random prefab

            RaycastHit hit;
            Ray ray = new Ray(new Vector3(point.x, 300, point.y), Vector3.down);
            if (Physics.Raycast(ray, out hit, 500))
            {   
                // for debug
                 gizmo.Add(hit.point);
            }

            GameObject tree = Instantiate(floraPrefabs[index], this.transform.InverseTransformPoint(hit.point), Quaternion.identity, this.transform) as GameObject;
            tree.transform.Rotate(0, Random.Range(0, 360), 0);

            tree.transform.localScale = Vector3.one * Random.Range(tree.transform.localScale.x * floraPrefabsScaleMin, tree.transform.localScale.x * floraPrefabsScaleMax); // add in some scaling randomness for size diffs
            
        }
    }

    public void DestroyExisting()
    {
        // destroy existing
        GameObject[] clones = GameObject.FindGameObjectsWithTag("Flora");
        foreach (var clone in clones)
        {
            DestroyImmediate(clone);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(sampleRegionSize / 2, sampleRegionSize);
        if (gizmo != null)
        {
            foreach (Vector3 point in gizmo)
            {
                Gizmos.DrawSphere(point, 1);
            }
        }
    }

     public void highest() // gets highest point on the mesh in local space, using provided gameobject...
    {
        float highest = 0;
        for (int y = 0; y < sampleRegionSize.y; y++){
            for (int x = 0; x < sampleRegionSize.x; x++){
                RaycastHit hit;
                Ray ray = new Ray(new Vector3(x, 300, y), Vector3.down);
                if (terrainPlaneObj.GetComponent<Collider>().Raycast(ray, out hit, 500)){
                    if (terrainPlaneObj.transform.InverseTransformPoint(hit.point).y > highest) highest = terrainPlaneObj.transform.InverseTransformPoint(hit.point).y;
                }
            }
        }
        Debug.Log("highest y is:   "+highest);
        highestPoint = highest;
    }



}
