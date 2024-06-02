using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProcGenTiles;


public class RoadGen : MonoBehaviour
{
    [SerializeField] LayerTerrain lt;
    [SerializeField] Color roadColor;
    public Pathfinding pathFinding;
    //public ConvexHull convexHull;

    private float[,] noiseMap;
    private Color[] colorMap;

    private float[,] roadMapData;
    private List<(int, int)> entryPoints = new List<(int, int)>();
    List<List<(int x, int xy)>> paths;
    List<List<(int x, int xy)>> badPaths;
    int width;
    int height;

    public bool showPaths = false;
    public bool showEntryPoints = false;
    public bool showFloodfill = false;
    public int FloodfillSizeColorLimit = 0;

    Dictionary<Vector3[], Color> gizmoPointsDict;

    public int entryGapMin = 15;
    [SerializeField] public float elevationLimitForPathfind;

    /* 
       1) Find map entries: loop around exterior of terrain. Find all of the low-elevation levels.
               The goal is to find the longest distance from these points to another point, with the least amount of curve.
               This is the arterial road(s). Think highways going through a city...
        
       2) Pathfind between each point using only flat terrain (for now) to keep things simple.
               Need to pathfind start/end each found entry point.
                    For each node along path, run a normal to the nearest opposite landmass (if something close within X meters).
                    The pathfinding will follow the edge of a mountain, which is fine, but if it's going between two landmasses we want it centered between the two.
               Find one or two roads that are the longest and most interesting
     
     I don't think I have it saving ALL the map settings yet, so here's my settings:
    ---Layer Terrain ---
    Draw Mode:     Color Map
    Draw Type:     Terrain
    Terrain Size:  256
    X :            256
    Y :            256
    Depth :        10
    
    ElevationLayers: Noise Pairs = 2
    (both of them are the exact same)
    NoiseType:             Perlin
    Seed:                  10
    NoiseScale:            8
    Frequency:             0.0014
    FractalType:           PingPong
    Octave:                5
    Gain:                  0.432
    Lacunarity:            2.65
    Weighted Strength:     0
    RaisedPower:           8
    Min Value:             0.18

    I'm going to save these ^^^ in a json called 'Pathfinding.json' in my commit,
    Just keep in mind there's a rounding error when it's being serialized, 
    so ur results might differ? 
    my frequency of: 0.0014
    becomes:         0.00139999995008111

    Moisture Layers aren't being used rn, shouldn't matter.


    --- Road Gen ---
    Entry Gap Min:                 20
    Elevation Limit For Pathfind:  0.02

 */


    public Color[] GetArterialPaths(float[,] noisyMcNoiseFace, Color[] colorMcMapFace)
    {
        Debug.Log("=============== RoadGen GetArterialPaths() =======================");

        // setting values
        noiseMap = noisyMcNoiseFace;
        colorMap = colorMcMapFace;
        width = noiseMap.GetLength(0);
        height = noiseMap.GetLength(1);
        pathFinding = new Pathfinding(lt.finalMap);
        roadMapData = new float[width, height];


        //entryPoints = GetMapEntries(); // this will populate entry-points automatically
        // need to fix no-path before this will work
        // (makes while loop run forever)

        // this is just for debug, only works for 256x256 map
        if (showEntryPoints)
        {
            entryPoints.Add((0, 10));
            /*
            entryPoints.Add((132, 0));
            entryPoints.Add((0, 177));
            entryPoints.Add((142, 255));
            entryPoints.Add((0, 88));
            entryPoints.Add((192, 0));*/
            //entryPoints.Add((142, 255)); // this is an entry with no exit, for testing no-path exits. TODO: fix no path exits lmao
            //entryPoints.Add((255, 117)); // this is an entry with no exit, for testing no-path exits. TODO: fix no path exits lmao

            //Debug.Log($"found {entryPoints.Count} entryPoints");

            // color map entry points
            foreach ((int x, int y) points in entryPoints)
            {
                DrawColorAtPoint(points.Item1, points.Item2, Color.cyan);
            }
        }

        if (showPaths)
        {
            paths = PathfindEachEntry(entryPoints);

            // draw paths
            foreach (List<(int x, int y)> path in paths) //draw paths
            {
                DrawPathsOnColorMap(path, Color.green, true);
            }

            // draw bad paths (if applicable)
            foreach (List<(int x, int y)> path in badPaths)
            {
                DrawPathsOnColorMap(path, Color.red, true);
            }
        }
       

        if (showFloodfill)
        {
            List<List<Tile>> allRegions = pathFinding.MarkLandmassRegions(noiseMap, elevationLimitForPathfind);
            Color[] someColors = { Color.blue, Color.grey, Color.green, Color.red, Color.magenta,
            Color.yellow, Color.cyan, Color.black, Color.white };
            int colorIndex = 0;
            gizmoPointsDict = new Dictionary<Vector3[], Color>();

            foreach (List<Tile> region in allRegions)
            { 
                if (region.Count >= FloodfillSizeColorLimit)
                {
                    //Color each region with a unique color
                    Color drawColor = someColors[colorIndex];
                    foreach (Tile t in region)
                    {
                        DrawColorAtPoint(t.x, t.y, drawColor);
                    }

                    drawHullGizmos(drawColor); //passing in the color so that we can color them the same

                    colorIndex++;
                    if (colorIndex >= someColors.Length - 1)
                    {
                        colorIndex = 0;
                    }

                    void drawHullGizmos(Color drawColor)
                    {   
                        // TODO: this is fucking DISGUSTING dont @ me lmao
                        List<(int x, int y)> tilesasTuple = new List<(int x, int y)>();
                        foreach (Tile t in region) tilesasTuple.Add((t.x, t.y));
                        List<(int x, int y)> hullPoints = ConvexHull.GetConvexHull(tilesasTuple);

                        Vector3[] gizmoPoints = new Vector3[hullPoints.Count];

                        for (int i = 0; i < hullPoints.Count; i++)
                        {
                            //gizmoPoints[i] = new Vector3((float)hullPoints[i].x, 1.0f, (float)hullPoints[i].y); //my fucked axis has finally fucked me back -_-
                            gizmoPoints[i] = new Vector3((float)hullPoints[i].y, 1.0f, (float)hullPoints[i].x);
                        }
                        gizmoPointsDict.Add(gizmoPoints, drawColor);
                    }

                }
                
            }
        }

        return colorMap; //returns map to gamemanger, which applies texture

    }


    /// <summary>
    /// Loops around edges of noiseMap, finds entry points based on elevation.
    /// Returns a List<(int x, int y)> of entry points into the map, using 'entryGapMin' value.
    /// </summary>
    private List<(int, int)> GetMapEntries()
    {
        // TODO: Make this not stupid
        // TODO: Fix bug where it doesn't get the right size of gap sometimes?
        // TODO: Implement where when it gets to the corner,
        //       if it's currently in a gap, continue the gap size.    <--- implemented but not tested


        int lengthBottomSegmentGap = 0;
        int lengthTopSegmentGap = 0;
        int lengthLeftSegmentGap = 0;
        int lengthRightSegmentGap = 0;

        // bottom left (0,0) --> bottom right (0,length)
        for (int col = 0; col < width; col++)
        {
            if (noiseMap[0, col] <= elevationLimitForPathfind)
            {
                roadMapData[0, col] = 0f;
                colorMap[0 * width + col] = Color.black; //works
                lengthBottomSegmentGap += 1;
            }
            else
            {
                if (lengthBottomSegmentGap > 0)
                {
                    if (lengthBottomSegmentGap >= entryGapMin)
                    {
                        int pointY = col - (lengthBottomSegmentGap / 2);
                        entryPoints.Add((0, pointY));
                        lengthBottomSegmentGap = 0;
                    }
                }
            }
        }

        // bottom-right (0,length) --> top right (height, length)
        for (int row = 1; row < height - 1; row++)
        {
            if (lengthBottomSegmentGap > 0) lengthRightSegmentGap = lengthBottomSegmentGap;
   
            if (noiseMap[row, width - 1] <= elevationLimitForPathfind)
            {
                roadMapData[row, width - 1] = 0f;
                colorMap[row * width - 1] = Color.black;
                lengthRightSegmentGap += 1;
            }
            else
            {
                if (lengthRightSegmentGap > 0)
                {
                    if (lengthRightSegmentGap >= entryGapMin)
                    {
                        int pointY = row - (lengthRightSegmentGap / 2);
                        entryPoints.Add((pointY, width - 1));
                        lengthRightSegmentGap = 0;
                    }
                }
            }

        }

        // top-right (height, length) --> top left (height,0)
        for (int col = width - 1; col > 0; col--)
        {
            if (lengthRightSegmentGap > 0) lengthTopSegmentGap = lengthRightSegmentGap;

            if (noiseMap[height - 1, col] <= elevationLimitForPathfind)
            {
                roadMapData[height - 1, col] = 0f;
                colorMap[(width - 1) * width + col] = Color.black; //works
                lengthTopSegmentGap += 1;
            }
            else
            {
                if (lengthTopSegmentGap > 0)
                {
                    if (lengthTopSegmentGap >= entryGapMin)
                    {
                        int pointY = col - (lengthTopSegmentGap / 2);
                        entryPoints.Add((width - 1, pointY));
                        lengthTopSegmentGap = 0;
                    }
                }
            }
        }

        // top-left (height,0) to bottom-right (0,0)
        for (int row = height - 1; row > 0; row--)
        {
            if (lengthTopSegmentGap > 0) lengthLeftSegmentGap = lengthTopSegmentGap;

            if (noiseMap[row, 0] <= elevationLimitForPathfind)
            {
                roadMapData[row, 0] = 0f;
                colorMap[row * width] = Color.black;
                lengthLeftSegmentGap += 1;
            }
            else
            {
                if (lengthLeftSegmentGap > 0)
                {
                    if (lengthLeftSegmentGap >= entryGapMin)
                    {
                        int pointY = row - (lengthLeftSegmentGap / 2);
                        entryPoints.Add((pointY, 0));
                        lengthLeftSegmentGap = 0;
                    }
                }
            }
        }

        return entryPoints;
    }

    //-----------------------------------------------------------------------------------------
    /// <summary>
    /// Returns List<List<(int x, int xy)>> of pathfinds into the map.
    /// </summary>
    private List<List<(int x, int xy)>> PathfindEachEntry(List<(int x, int y)> entryPoints)
    {
        paths = new List<List<(int x, int xy)>>();
        badPaths = new List<List<(int x, int xy)>>();

        for (int i = 0; i < entryPoints.Count - 1; i++)
        {
            for (int j = 1; j < (entryPoints.Count - 1) - i; j++)
            {
                // if x or y is on the same border side, go to next
                if (entryPoints[i].Item1 == entryPoints[i + j].Item1) continue;
                if (entryPoints[i].Item2 == entryPoints[i + j].Item2) continue;

                (int x, int y) xy_end = entryPoints[i + j];

                Dictionary<string, List<(int x, int y)>> pathData = pathFinding.AStar(entryPoints[i], xy_end, noiseMap, elevationLimitForPathfind);
                List<(int x, int xy)> foundPath = pathData["path"];
               
                if (foundPath != null)
                {
                    //Debug.Log($"Path from {entryPoints[i]} to {xy_end} has length of {foundPath.Count}");
                    paths.Add(foundPath);

                    if (pathData.ContainsKey("badPaths")) 
                    { 
                        List<(int x, int xy)> bad = pathData["badPaths"]; 
                        badPaths.Add(bad); 
                    }
                } else Debug.Log($"No path for {entryPoints[i]} to {xy_end} !!!!!!");
            }
        }

        return paths;
    }


    public void DrawPathsOnColorMap(List<(int x, int y)> toDraw, Color color, bool ShowAll = true)
    {
        if (ShowAll)
        {
            foreach ((int x, int y) points in toDraw)
            {
                DrawColorAtPoint(points.Item1, points.Item2, color);
            }
        } else
        {   
            // I was cooking but i forgor why    
        }

    }


    public void DrawColorAtPoint(int x, int y, Color color)
    {
        colorMap[x * width + y] = color;
    }

    void OnDrawGizmosSelected()
    {
        if (gizmoPointsDict != null)
        {
            foreach (KeyValuePair<Vector3[], Color> gizmoPoints in gizmoPointsDict)
            {
                Gizmos.color = gizmoPoints.Value;
                Gizmos.DrawLineStrip(gizmoPoints.Key, true);
            }
        }
    }
}
