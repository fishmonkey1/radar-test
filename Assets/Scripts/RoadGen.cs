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

    public bool autoUpdate = true;

    private float[,] noiseMap;
    private Color[] colorMap;

    private float[,] roadMapData;


    int width;
    int height;

    public bool showPaths = false;
    public bool showEntryPoints = false;
    public bool showFloodfill = false;
    public bool showConvexHull = false;
    [Tooltip("Limits the region drawing to regions larger than the minimum")]
    public int floodfillRegionMinimum = 0;

    Dictionary<Vector3[], Color> gizmoPointsDict;

    public int entryGapMin = 15;
    [SerializeField] public float elevationLimitForPathfind = .02f;

    Color[] someColors = { Color.blue, Color.grey, Color.green, Color.red, Color.magenta,
            Color.yellow, Color.cyan, Color.black, Color.white };

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
        gizmoPointsDict = new Dictionary<Vector3[], Color>();

        List<(int x, int xy)> entryPoints = new List<(int, int)>();
        List<List<Tile>> paths = new List<List<Tile>>();
        List<List<Tile>> allRegions = new List<List<Tile>>();
        //List<List<(int x, int xy)>> badPaths = new List<List<(int x, int xy)>>();


        entryPoints.Add((142, 255));
        entryPoints.Add((132, 0));
        entryPoints.Add((192, 0));
        entryPoints.Add((255, 175));
        entryPoints.Add((0, 30));
        entryPoints.Add((0, 177));
        entryPoints.Add((0, 88));


        if (showFloodfill || showConvexHull) //if doing either we need the regions
        {
            allRegions = pathFinding.MarkLandmassRegions(noiseMap, elevationLimitForPathfind);

            int colorIndex = 0;
            //gizmoPointsDict = new Dictionary<Vector3[], Color>();
            foreach (List<Tile> region in allRegions)
            {
                if (region.Count >= floodfillRegionMinimum)
                {
                    Color drawColor = someColors[colorIndex];

                    if (showFloodfill)
                    {
                        foreach (Tile t in region)
                        {
                            DrawColorAtPoint(t.x, t.y, drawColor);
                        }
                        Debug.Log($"Length of region {region[0].ValuesHere["Region"]} is {region.Count}");
                    }

                    if (showConvexHull)
                    {
                        drawHullGizmos(drawColor);
                    }

                    colorIndex++;
                    if (colorIndex >= someColors.Length - 1)
                    {
                        colorIndex = 0;
                    }

                    void drawHullGizmos(Color drawColor)
                    {
                        List<(int x, int y)> hullPoints = ConvexHull.GetConvexHull(region); // TODO: Can you edit to have this return a List<Tile> instead?
                                                                                            //       I had a hard time figuring it out.
                                                                                            //       Obvs we can just iterate over the tuples at the end before returning,
                                                                                            //       but if we create tiles instead of tuples hell yeah.

                        if (hullPoints != null)
                        {
                            Vector3[] gizmoPoints = new Vector3[hullPoints.Count]; //idk why but need to do it like this for the Gizmo stuff?

                            for (int i = 0; i < hullPoints.Count; i++)
                            {
                                gizmoPoints[i] = new Vector3((float)hullPoints[i].y, 1.0f, (float)hullPoints[i].x);
                            }

                            gizmoPointsDict.Add(gizmoPoints, drawColor);
                        }
                    }
                }
            }

            BisectMap(allRegions);

        }



        if (showPaths)
        {

            //entryPoints = GetMapEntries(); // this will populate entry-points automatically
            // need to fix no-path before this will work
            // (makes while loop run forever)


            // these are just for debug, only works for 256x256 map
            /*entryPoints.Add((142, 255));  // gets stuck
            entryPoints.Add((132, 0));
            entryPoints.Add((192, 0));
            entryPoints.Add((255, 175));
            entryPoints.Add((0, 30));
            entryPoints.Add((0, 177));
            entryPoints.Add((0, 88));*/

            //entryPoints.Add((255, 117)); // this is an entry with no exit, for testing no-path exits. TODO: fix no path exits lmao

            PathfindEachEntry(entryPoints, paths);

            int colorIndex = 0;
            foreach (List<Tile> path in paths)
            {
                DrawPathsOnColorMap(path, someColors[colorIndex], true);
                colorIndex++;
                if (colorIndex >= someColors.Length - 1)
                {
                    colorIndex = 0;
                }

            }

            // draw bad paths (if applicable)
            /*            foreach (List<(int x, int y)> path in badPaths)
                        {
                            DrawPathsOnColorMap(path, Color.red, true);
                        }*/
        }

        if (showEntryPoints)
        {
            if (!showPaths)
            {
                //entryPoints.Add((0, 30));
                //entryPoints.Add((142, 255));
                // entryPoints.Add((132, 0));
            }

            // color map entry points
            foreach ((int x, int y) points in entryPoints)
            {
                DrawColorAtPoint(points.Item1, points.Item2, Color.cyan);

                Tile t = lt.finalMap.GetTile(points.x, points.y);
                var border = pathFinding.GetNeighbors(points, pathFinding.TileOverElevation, true, null, -999f);
                foreach (Tile tt in border) DrawColorAtPoint(tt.x, tt.y, Color.cyan);
            }

            /*foreach (List<Tile> path in paths) //draw end node
            {
                if (showEntryPoints)
                {
                    Tile last = path[path.Count - 1];
                    DrawColorAtPoint(last.x, last.y, Color.cyan);
                }
            }*/
        }

        return colorMap; //returns map to gamemanger, which applies texture


    }

    private void BisectMap(List<List<Tile>> allRegions)
    {
        List<List<Tile>> landmasses = new List<List<Tile>>();

        //Create List of landmasses we are going to be navigating between
        // ignore anything smaller than the set min
        foreach (List<Tile> region in allRegions)
        {
            if (region.Count >= floodfillRegionMinimum)
            {
                landmasses.Add(region);
            }
        }

        // create a list of regions that are neighbors
        // I know this is a mess lol I had to start somewhere :3
        for (int i = 0; i < landmasses.Count - 1; i++)
        {
            List<List<Tile>> regionNeighbors_n = null; // going to be using hullpoints, not the whole list
            List<List<Tile>> regionNeighbors_e = null;
            List<List<Tile>> regionNeighbors_s = null;
            List<List<Tile>> regionNeighbors_w = null;

            for (int j = i+1; j < landmasses.Count; j++) // compare every landmass to every one in front of it in the List. 
            {                                            // This compares everything with everything else, without duplicates.
                List<Tile> currentRegion = landmasses[i];
                List<Tile> compareToRegion = landmasses[j];

                (int curr_n, int curr_e, int curr_s, int curr_w) = GetBounds(currentRegion); // I'm going to pass in the hullpoints just bear with me lol
                (int comp_n, int comp_e, int comp_s, int comp_w) = GetBounds(compareToRegion);

                if ((curr_s <= comp_s && comp_s <= curr_n) || (curr_n >= comp_n && comp_n >= curr_s || (curr_n < comp_n && curr_s > comp_s))) // compareToRegion is on the east or west of current region
                {
                    if (curr_e < comp_e) regionNeighbors_e.Add(compareToRegion); //it's an east neighbor
                    if (curr_w > comp_e) regionNeighbors_w.Add(compareToRegion); // it's a west neighbor
                }

                if ((curr_w <= comp_w && comp_w <= curr_e) || (curr_e >= comp_e && comp_e >= curr_w || (curr_e < comp_e && curr_w > comp_w))) // compareToRegion is on the north or south of current region
                {
                    if (curr_n < comp_n) regionNeighbors_n.Add(compareToRegion); //it's an north neighbor
                    if (curr_s > comp_s) regionNeighbors_s.Add(compareToRegion); // it's a south neighbor
                }
            }

            // After done looping, store those values in the Region object for each region on the Map (that still needs to be created) 

            // At this point, you have a list of what regions are around you (4 neighbors style, but extending all the way out).
            // now need to see which of those are CLOSEST (immediately to the sides)
            /*
                for row-or-column-both-regions-are-occupying:
                    get distance between the two along the row/column
                    get midpoint of that distance.
                    mark midpoint. 
                    (Could also do some slope math here with the hull points but brain no worky rn)
                  
                Now we have a line of dots between regions. Get the best-fit line for those midpoints.
                Save the tentative lines.     
               -- 
                if you have multiple lines spaced apart in a line w/ ~same slope, make them a single line. (average out all the points to get line)
                Extend lines out to find intersect points between lines... We are going to use these to make the bezier curves for the road. Really just extend out until it hits something.

                If you've actually coded and implemented this far, go pour a fuckin drink cuz you probs wanna blow your brains out from frustration. 

                
                  
                 
             */

        }

        (int n, int e, int s, int w) GetBounds(List<Tile> region)
        {
            int n = 0;
            int e = 0;
            int s = height + 1;
            int w = width + 1;

            // set bounds values
            foreach (Tile t in region)
            {
                if (t.y > n) n = t.y; // Get n bound (highest Y)
                if (t.x > e) e = t.x; // Get e bound (highest x)
                if (t.y < s) s = t.y; // Get s bound (lowest Y)
                if (t.x < w) w = t.x; // Get w bound (lowest x)
            }
            return (n, e, s, w);
        }

    }

    /// <summary>
    /// Loops around edges of noiseMap, finds entry points based on elevation.
    /// Returns a List<(int x, int y)> of entry points into the map, using 'entryGapMin' value.
    /// </summary>
    private void GetMapEntries(List<(int, int)> entryPoints)
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
    }

    //-----------------------------------------------------------------------------------------
    /// <summary>
    /// Returns List<List<(int x, int xy)>> of pathfinds into the map.
    /// </summary>
    /*private List<List<(int x, int xy)>> PathfindEachEntry(List<(int x, int y)> entryPoints)
    {*/
    private void PathfindEachEntry(List<(int x, int y)> entryPoints, List<List<Tile>> paths)
    {
        Debug.Log(entryPoints.Count);
        for (int i = 0; i < entryPoints.Count - 1; i++) //stop at second-to-last
        {
            for (int j = i + 1; j < (entryPoints.Count); j++)
            {
                // if x or y is on the same border side, go to next
                (int x, int y) end = entryPoints[j];
                if (entryPoints[i].x == end.x) continue;
                if (entryPoints[i].y == end.y) continue;


                //Dictionary<string, List<(int x, int y)>> pathData = pathFinding.AStar(entryPoints[i], end, noiseMap, elevationLimitForPathfind);
                //List<(int x, int xy)> foundPath = pathData["path"];
                Tile startT = lt.finalMap.GetTile(entryPoints[i].x, entryPoints[i].y);
                Tile endT = lt.finalMap.GetTile(end.x, end.y);
                List<Tile> path = pathFinding.new_Astar(startT, endT, elevationLimitForPathfind);

                if (path != null)
                {
                    Debug.Log($"Path from {entryPoints[i]} to {end} has length of {path.Count}");
                    paths.Add(path);
                }
                else Debug.Log($"No path for {entryPoints[i]} to {end} !!!!!!");
            }
        }

    }


    public void DrawPathsOnColorMap(List<Tile> toDraw, Color color, bool ShowAll = true)
    {
        if (ShowAll)
        {
            foreach (Tile points in toDraw)
            {
                DrawColorAtPoint(points.x, points.y, color);
            }
        }
        else
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
        if (showConvexHull)
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

    public void runMapGen() //this is so the Editor script can auto update on changes
    {
        lt.gameManager.loadNewData();
    }





}


