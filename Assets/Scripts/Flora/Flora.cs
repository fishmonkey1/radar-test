using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using ProcGenTiles;
using KaimiraGames;

public class Flora : MonoBehaviour
{

    [SerializeField] public Vector2 sampleRegionSize = Vector2.one;
    [SerializeField] int numSamplesBeforeRejection = 30;
    [SerializeField] float floraPrefabsScaleMin;
    [SerializeField] float floraPrefabsScaleMax;

    Map floraMap;

    public bool genFlora = false;
    public bool EditNoise = true;
    List<Vector3> gizmo = new List<Vector3>();
    Dictionary<Vector3, Zone> gizmoDict = new Dictionary<Vector3, Zone>();

    [SerializeField] public float scale = 1.0F;
    [SerializeField] [Range(0f, 1f)] public float step = .5f;

    [SerializeField] Color sand; //CBBD93
    [SerializeField] Color grass = Color.green;
    public bool colorGrass;

    public int[,] grassNoiseMap;

    private Texture2D noiseTex;
    private Texture2D noNoiseTex;
    private Color[] pix;
    private Renderer rend;

    Zone grassZone;

    [SerializeField] public GameObject terrainPlaneObj;

    [SerializeField] List<Zone> Zones = new List<Zone>();

    Vector2 defaultStartPoint;

    // highest() gets highest Y val on mesh, just going to use this to do quick and dirty elevation maffs for the spawner
    float highestPoint = 50; 

    private void Awake()
    {
        DestroyExisting();
        defaultStartPoint = sampleRegionSize / 2;
        grassNoiseMap = new int[(int)sampleRegionSize.x, (int)sampleRegionSize.y];

    }
    private void Start()
    {  
        //GenPSD();
    }

    public void GenGrass()
    {
        if (floraMap == null) floraMap = new Map((int)sampleRegionSize.x, (int)sampleRegionSize.y); 
        //grassNoiseMap = new int[(int)sampleRegionSize.x, (int)sampleRegionSize.y];
        grassZone = GetGrassZone();
        CaclulatePDSandPlaceGrass();

        //uncomment to do both grass and other
        //GenPSD(defaultStartPoint);

    }
 
    public void GenPDS(Vector2 startPoint, Zone zoneFilter=null)
    //public void CalculateSpawnFlora()
    {   

        InitZonesWeighted();

        List<Vector2> points = new List<Vector2>();
        List<Vector2> spawnPoints = new List<Vector2>(); //when added to points, added to spawnpoints, if fails removes spanpoint

        spawnPoints.Add(startPoint);

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

            Zone currZone = GetZone(spawnCentre);
            float currentMinRadius = currZone.minDensityPSD;
            float currentMaxRadius = currZone.maxDensityPSD;

            for (int i = 0; i < numSamplesBeforeRejection; i++)
            {
                // gets a random angle and direction to find a new point on
                float angle = Random.value * Mathf.PI * 2;
                Vector2 dir = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));

                // get candidate point based on max/min of current point
                Vector2 candidate = spawnCentre + dir * Random.Range(currentMinRadius, currentMaxRadius);
                
                float candidateMinRadius = -1f;
                float candidateMaxRadius = -1f;

                // need to check if point isvalid before tyrying to check zone
                if (IsValid(candidate, sampleRegionSize, cellSize, candidateMinRadius, candidateMaxRadius, points, grid, zoneFilter))
                {
                    
                    Zone candidateZone = GetZone(candidate);
                    

                    if (candidateZone != null)
                    {
                        candidateMinRadius = candidateZone.minDensityPSD;
                        candidateMaxRadius = candidateZone.maxDensityPSD;
                    }
                    else
                    {
                        Debug.Log("candidate has no zone, skipping");
                        continue;
                    }

                    if (zoneFilter != null)
                    {
                        if (candidateZone != zoneFilter)
                        {
                            continue;
                        }
                    }

                    // check surrounding cells of candidate
                    // to make sure that there are no points that would invalidate it
                    if (IsValid(candidate, sampleRegionSize, cellSize, candidateMinRadius, candidateMaxRadius, points, grid, zoneFilter))
                    {
                        SpawnObjectInZone(candidate, candidateZone);

                        spawnPoints.Add(candidate);
                        points.Add(candidate);
                        grid[(int)(candidate.x / cellSize), (int)(candidate.y / cellSize)] = points.Count;
                        candidateAccepted = true;
                        break;
                    }
                    
                } else
                {
                    //candidate not valid:
                }
            }
            if (!candidateAccepted)
            {
                spawnPoints.RemoveAt(spawnIndex);
            }

        }
        

        // checks the surrounding cells around a candidate
        // to make sure there aren't any too close to it which would invalidate it
        bool IsValid(Vector2 candidate, Vector2 sampleRegionSize, float cellSize, float minRadius, float maxRadius, List<Vector2> points, int[,] grid, Zone zoneFilter = null)
        {

            // check if candidate is within the sample region on the map
            if (candidate.x >= 0 && candidate.x < sampleRegionSize.x && candidate.y >= 0 && candidate.y < sampleRegionSize.y)
            {   
                // check if it's in a zone
                if (GetZone(candidate) == null)
                {
                    return false;
                }

                //check against zoneFilter
                if (zoneFilter != null)
                {
                    if (GetZone(candidate).name != zoneFilter.name)
                    {
                        //Debug.Log("candidateZone != zonefilter");
                        return false;
                    }
                }
                


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
    }


    public bool isGrassAtLocation(Vector2 location)
    {   
        if (grassNoiseMap[(int)location.x, (int)location.y] != 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public Zone GetZone(Vector2 location)
    {
        float elevationYlocal = GetLocalY(location.x, location.y, terrainPlaneObj);
        float candidateElevation = Mathf.InverseLerp(0f, highestPoint, elevationYlocal);

        if (isGrassAtLocation(location))
        {
            return GetGrassZone();
        }

        foreach (Zone zone in Zones)
        {   // for now selecting zone based on elevation
            if (!zone.IsGrassZone)
            {
                if (zone.elevationMin <= candidateElevation && candidateElevation < zone.elevationMax)
                {
                    return zone;
                }
            }
            
        }
        return null;
    }
    
   

    Zone GetGrassZone()
    {
        foreach (Zone zone in Zones)
        {
            if (zone.IsGrassZone) return zone;
        }
        Debug.Log("No grass zone created, please create one!");
        return null;
    }

    public void CaclulatePDSandPlaceGrass()
    {
        if (grassZone == null)
        {
            grassZone = GetGrassZone();
        }
        if (grassNoiseMap == null)
        {
            CalculateGrassNoise(false);
        }

        for (int x = 0; x < sampleRegionSize.x; x+=8) 
        {
            for (int y = 0; y < sampleRegionSize.y; y+=8)
            { 

                if (grassNoiseMap[x,y] != 0)
                {
                    // we got grass, now run PSD
                    GenPDS(new Vector2(y,x), grassZone);
 
                } 
            }
        }
    }

    public void CalculateGrassNoise(bool DrawNoise=false, bool UndoDrawNoise=false)
    {
        if (UndoDrawNoise) { RevertTexture(); return; }

        if (grassZone==null)
        {
            Debug.Log("No grassZone, cannot spawn grass");
            return;
        }

        grassNoiseMap = new int[(int)sampleRegionSize.x, (int)sampleRegionSize.y];

        int pixWidth = (int)sampleRegionSize.x;
        int pixHeight = (int)sampleRegionSize.y;
        rend = terrainPlaneObj.GetComponent<Renderer>();

        noiseTex = new Texture2D(pixWidth, pixHeight);
        pix = new Color[noiseTex.width * noiseTex.height];
        if (noNoiseTex == null && EditNoise == true) { noNoiseTex = (Texture2D)rend.sharedMaterial.mainTexture; } //nab and store original before changing
        rend.sharedMaterial.mainTexture = noiseTex;
        

        

        // For each pixel in the texture...
        for (float y = 0.0f; y < noiseTex.height; y++)
        {
            for (float x = 0.0f; x < noiseTex.width; x++)
            {
                float xCoord = x / noiseTex.width * scale;
                float yCoord = y / noiseTex.height * scale;
                float sample = Mathf.PerlinNoise(xCoord, yCoord);


                if (sample <= step)
                {
                    pix[(int)y * noiseTex.width + (int)x] = grass;
                    grassNoiseMap[(int)x, (int)y] = 1; // set from 0 --> 1 if grass
                    if (DrawNoise)
                    {
                        grassNoiseMap[(int)x, (int)y] = 1; // set from 0 --> 1 if grass
                    }
                }
                else
                {
                    if (DrawNoise)
                    {
                        pix[(int)y * noiseTex.width + (int)x] = sand;
                    }
                }

            }
        }

        // Copy the pixel data to the texture and load it into the GPU.
        noiseTex.SetPixels(pix);
        noiseTex.Apply();

        void RevertTexture()
        {   
            rend = terrainPlaneObj.GetComponent<Renderer>();
            pix = new Color[noiseTex.width * noiseTex.height];
            for (int i = 0; i < pix.Length; ++i)
            {
                pix[i] = sand;
            }
            noNoiseTex = new Texture2D(noiseTex.width, noiseTex.width);
            rend.sharedMaterial.mainTexture = noNoiseTex;
            noNoiseTex.SetPixels(pix);
            noNoiseTex.Apply();
        }

    }

    public void SpawnObjectInZone(Vector2 location, Zone zone)
    {
        //Debug.Log("got to spawninzone");
        RaycastHit hit;
        Ray ray = new Ray(new Vector3(location.x, 300, location.y), Vector3.down);
        if (Physics.Raycast(ray, out hit, 500))
        {   
            // TODO: Need to check that it doesn't hit road/building
            //       probs do that in IsValid()

            // for debug
            gizmo.Add(hit.point);
            gizmoDict.Add(hit.point, zone);
        }

        GameObject spawnedObject = Instantiate(ChooseFlora(), this.transform.InverseTransformPoint(hit.point), Quaternion.identity, this.transform) as GameObject;
        spawnedObject.transform.Rotate(0, Random.Range(0, 360), 0);
        if (zone.IsGrassZone)
        {
            spawnedObject.transform.localScale = Vector3.one * Random.Range(spawnedObject.transform.localScale.x * .1f, spawnedObject.transform.localScale.x * .3f); // add in some scaling randomness for size diffs
        }
        else
        {
            spawnedObject.transform.localScale = Vector3.one * Random.Range(spawnedObject.transform.localScale.x * floraPrefabsScaleMin, spawnedObject.transform.localScale.x * floraPrefabsScaleMax); // add in some scaling randomness for size diffs
        }

        GameObject ChooseFlora()
        {
            // https://github.com/cdanek/KaimiraWeightedList/tree/main
            GameObject selected = zone.weightedObjects.Next();
            if (selected == null) { Debug.Log("ChooseFlora() no object chosen :("); }
            //Debug.Log(selected.name+ "  spawned");
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
        foreach (KeyValuePair<Vector3, Zone> point in gizmoDict)
        {
            Gizmos.DrawWireSphere(point.Key, point.Value.minDensityPSD);
        }

    }

    public void InitZonesWeighted()
    {
        foreach (Zone zone in Zones)
        {
            foreach (ZoneObject zoneObj in zone.SpawnedObjects)
            {
                zone.weightedObjects.Add(zoneObj.spawnObject, (int)zoneObj.probability * 100);
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
