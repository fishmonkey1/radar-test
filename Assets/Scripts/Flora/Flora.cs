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

   

   /* public void GenPSD2()    old dumb stupid code we're not using anymore cuz its stupid :3
    {
        //highest();
        points = PoissonDiscSampling.GeneratePoints(minRadius, maxRadius, sampleRegionSize, numSamplesBeforeRejection, null, Zones);
        
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
    }*/

    public void GenPSD()
    //public void CalculateSpawnFlora()
    {
        List<Vector2> points = new List<Vector2>();
        List<Vector2> spawnPoints = new List<Vector2>(); //when added to points, added to spawnpoints, if fails removes spanpoint

        spawnPoints.Add(sampleRegionSize / 2); //add random spawnpoint to start at

        // radius is diagonal of cell, we need the size of each edge of the square cell
        // float cellSize = minRadius / Mathf.Sqrt(2);
        float cellSize = 1f / Mathf.Sqrt(2); //just going to manually set the base cellSize based on 1r

        // make array of ints (zeros) in size of sampleRegion. so if cell is 2 and region is 300, it would be 150x150 array 
        int[,] grid = new int[Mathf.CeilToInt(sampleRegionSize.x / cellSize), Mathf.CeilToInt(sampleRegionSize.y / cellSize)];

        // main loop
        while (spawnPoints.Count > 0)
        {

            //get random spawnCentre from spawnPoints
            int spawnIndex = Random.Range(0, spawnPoints.Count);
            Vector2 spawnCentre = spawnPoints[spawnIndex];

            bool candidateAccepted = false;

            // get random angle, see if it's valid, if is add to spawnpoints
            for (int i = 0; i < numSamplesBeforeRejection; i++)
            {
                // gets a random angle and direction to find a new point on
                float angle = Random.value * Mathf.PI * 2;
                Vector2 dir = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));

                // Seb has it as maxed at 2x the radius. 
                // We want to use the MaxRadius instead
                // figure max radius based on underlying point

                // get current point's min/max range values
                Zone currZone = GetZone(spawnCentre);
                float currentMinRadius = currZone.minDensityPSD;
                float currentMaxRadius = currZone.maxDensityPSD;

                Vector2 candidate = spawnCentre + dir * Random.Range(currentMinRadius, currentMaxRadius);

                Zone candidateZone = GetZone(candidate);
                float candidateMinRadius = candidateZone.minDensityPSD;
                float candidateMaxRadius = candidateZone.maxDensityPSD;

                if (candidateMinRadius != -1f) //if valid radius
                {   
                    // check surrounding cells of candidate
                    // to make sure that there are no points that would invalidate it
                    if (IsValid(candidate, sampleRegionSize, cellSize, candidateMinRadius, candidateMaxRadius, points, grid))
                    {
                        SpawnObjectInZone(candidate, candidateZone);
                        spawnPoints.Add(candidate);
                        points.Add(candidate);
                        grid[(int)(candidate.x / cellSize), (int)(candidate.y / cellSize)] = points.Count;
                        candidateAccepted = true;
                        break;
                    }
                }
            }
            if (!candidateAccepted)
            {
                spawnPoints.RemoveAt(spawnIndex);
            }

        }

        //return points;

        // checks the surrounding cells around a candidate
        // to make sure there aren't any too close to it which would invalidate it
        static bool IsValid(Vector2 candidate, Vector2 sampleRegionSize, float cellSize, float minRadius, float maxRadius, List<Vector2> points, int[,] grid)
        {

            // check if candidate is within the sample region on the map
            if (candidate.x >= 0 && candidate.x < sampleRegionSize.x && candidate.y >= 0 && candidate.y < sampleRegionSize.y)
            {
                // find which cell the point lies in so we can search surrounding cells		
                int cellX = (int)(candidate.x / cellSize);
                int cellY = (int)(candidate.y / cellSize);

                // this gets the bounds of the area we are searching.
                // the min/max is to make sure points aren't off map
                // Seb has it simply as -2 or +2 because his range is fixed between radius and radius*2,
                // We would want that to be based on the minRadius and maxRadius, so need to calculate number of squares for maxRadius (DONE!)
                // min/max so that we don't try to get cells off the edge of the map
                int searchSize = CalculateSearchArea(maxRadius);

                int searchStartX = Mathf.Max(0, cellX - searchSize);
                int searchEndX = Mathf.Min(cellX + searchSize, grid.GetLength(0) - 1);
                int searchStartY = Mathf.Max(0, cellY - searchSize);
                int searchEndY = Mathf.Min(cellY + searchSize, grid.GetLength(1) - 1);

                for (int x = searchStartX; x <= searchEndX; x++)
                {
                    for (int y = searchStartY; y <= searchEndY; y++)
                    {

                        // if pointIndex = -1 there is NO point in that spot
                        int pointIndex = grid[x, y] - 1;

                        // so if there is a point in the cell,
                        // check its distance and return IsValid=false if point is less than minRadius 
                        if (pointIndex != -1)
                        {
                            float sqrDst = (candidate - points[pointIndex]).sqrMagnitude; //sqrMagnitude is less expensive than Magnitude so doing this with squared values
                            if (sqrDst < minRadius * minRadius)
                            {
                                return false;
                            }
                        }
                    }
                }
                return true;
            }
            return false; //return false if point not on map

            int CalculateSearchArea(float maxRadius)
            {
                return Mathf.CeilToInt(maxRadius / cellSize);
            }
        }

        Zone GetZone(Vector2 location)
        {
            float elevationYlocal = GetLocalY(location.x, location.y, terrainPlaneObj);
            float candidateElevation = Mathf.InverseLerp(0f, 50f, elevationYlocal);

            foreach (Zone zone in Zones)
            {
                // for now selecting zone based on elevation
                if (zone.elevationMin <= candidateElevation && candidateElevation <= zone.elevationMax)
                {
                    return zone;
                }
            }
            return null;
        }

        
    }

    public void SpawnObjectInZone(Vector2 location, Zone zone)
    {
        RaycastHit hit;
        Ray ray = new Ray(new Vector3(location.x, 300, location.y), Vector3.down);
        if (Physics.Raycast(ray, out hit, 500))
        {   
            // TODO: Need to check that it doesn't hit road/building
            //       probs do that in IsValid()

            // for debug
            gizmo.Add(hit.point);
        }

        ZoneObject ChooseFlora()
        {   



            ZoneObject selected = zone.SpawnedObjects[0];
            return selected;
        }
    }


    public static float GetLocalY(float x, float y, GameObject obj)
    {
        RaycastHit hit;
        Ray ray = new Ray(new Vector3(x, 300, y), Vector3.down);
        if (obj.GetComponent<Collider>().Raycast(ray, out hit, 500))
        {
            return obj.transform.InverseTransformPoint(hit.point).y;
        }
        else return -999f;
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
