using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProcGenTiles;
using System.Linq;


public class RoadGen : MonoBehaviour
{
    [SerializeField] LayerTerrain lt;
    [SerializeField] Color roadColor;
    public Pathfinding pathFinding;
    public Map map;
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
    public bool showMidpoints = false;

    [Tooltip("Limits the region drawing to regions larger than the minimum")]
    public int floodfillRegionMinimum = 0;

    Dictionary<Vector3[], Color> gizmoPointsDict;

    public int entryGapMin = 15;
    [SerializeField] public float elevationLimitForPathfind = .02f;

    Color[] someColors = { Color.blue, Color.grey, Color.green, Color.red, Color.magenta,
            Color.yellow, Color.cyan, Color.black, Color.white };

    Dictionary<Color, string> someColorsdict = new Dictionary<Color, string> {
        { Color.blue, "blue"}, {Color.grey, "grey" }, { Color.green,  "green" },
        { Color.red, "red" }, { Color.magenta, "magenta" }, {  Color.yellow, "yellow" },
        { Color.cyan, "cyan" }, { Color.black, "black" }, { Color.white, "white" } };

    public List<List<(float, float)>> allmidpointsDEBUG = new List<List<(float, float)>>();



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
        map = lt.finalMap;
        pathFinding = new Pathfinding(map);
        roadMapData = new float[width, height];
        gizmoPointsDict = new Dictionary<Vector3[], Color>();




        List<(int x, int xy)> entryPoints = new List<(int, int)>();
        List<List<Tile>> paths = new List<List<Tile>>();
        List<Region> allRegions = new List<Region>();
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
            allRegions = pathFinding.MarkLandmassRegions(elevationLimitForPathfind);
            //Debug.Log($"successfully got {allRegions.Count} regions from floodfill");
            int colorIndex = 0;
            //gizmoPointsDict = new Dictionary<Vector3[], Color>();
            foreach (Region region in allRegions)
            {
                if (region.Tiles.Length >= floodfillRegionMinimum)
                {
                    region.Color = someColors[colorIndex];
                    region.colorName = someColorsdict[region.Color];

                    if (showFloodfill)
                    {
                        foreach (Tile t in region.Tiles)
                        {
                            DrawColorAtPoint(t.x, t.y, region.Color);
                        }
                        // Debug.Log($"Length of region {""} is {region.Tiles.Length}");
                    }


                    // TODO: Can you edit to have this return a List<Tile> instead?
                    //       Obvs we can just iterate over the tuples at the end before returning,
                    //       but if we create tiles instead of tuples hell yeah :3
                    List<(int x, int y)> hullPoints = ConvexHull.GetConvexHull(region.Tiles.ToList<Tile>());

                    if (hullPoints != null)
                    {
                        List<Tile> hpTiles = new List<Tile>(); // converting to tiles here, I know...
                        foreach ((int x, int y) point in hullPoints)
                        {
                            hpTiles.Add(map.GetTile(point));
                        }

                        region.HullPoints = hpTiles.ToArray(); //set the hullpoints for this Region.

                        // create and store hullpoint lines for region
                        region.HullLines = new (Tile, Tile)[region.HullPoints.Length];
                        for (int i = 0; i < region.HullPoints.Length - 1; i++) // loop through each point and combine w/ next-index point to create a line 
                        {                                                    // stop at 2nd to last, so we can set the last index to be matched w/ the first index after the for loop
                            region.HullLines[i] = (region.HullPoints[i], region.HullPoints[i + 1]);
                        }
                        region.HullLines[region.HullPoints.Length - 1] = (region.HullPoints[region.HullPoints.Length - 1], region.HullPoints[0]); // set the last line as: the Last and First point in the HullPoints array

                        drawHullGizmos(region.Color);
                    }

                    colorIndex++;
                    if (colorIndex >= someColors.Length - 1)
                    {
                        colorIndex = 0;
                    }

                    void drawHullGizmos(Color drawColor)
                    {

                        Vector3[] gizmoPoints = new Vector3[hullPoints.Count]; //idk why but need to do it like this for the Gizmo stuff?

                        int n = 0;
                        int e = 0;
                        int s = height + 1;
                        int w = width + 1;

                        //Region reg = map.GetRegion(hullPoints[0].x, hullPoints[0].y);

                        for (int i = 0; i < hullPoints.Count; i++)
                        {
                            gizmoPoints[i] = new Vector3((float)hullPoints[i].x, 1.0f, (float)hullPoints[i].y);

                            if (hullPoints[i].y > n) n = hullPoints[i].y; // Get n bound (highest Y)
                            if (hullPoints[i].x > e) e = hullPoints[i].x; // Get e bound (highest x)
                            if (hullPoints[i].y < s) s = hullPoints[i].y; // Get s bound (lowest Y)
                            if (hullPoints[i].x < w) w = hullPoints[i].x; // Get w bound (lowest x)


                        }

                        region.Bounds = new (int, int)[2];
                        region.Bounds[0] = (w, n);
                        region.Bounds[1] = (e, s);

                        gizmoPointsDict.Add(gizmoPoints, drawColor);

                    }
                }
            }

            FilterForLandmassRegions_GetMoreData();

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

                Tile t = map.GetTile(points.x, points.y);
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

    private void FilterForLandmassRegions_GetMoreData()
    {
        
        //Create List of landmasses we are going to be navigating between, ignore anything smaller than the set min
        // This shouldn't be done here, we should have a seperate array in Map for the "good" regions I guess.
        List<Region> landmasses = new List<Region>();
        foreach (Region region in map.Regions)
        {
            if (region.Tiles.Length >= floodfillRegionMinimum)
            {
                region.RegionNeighbors = new Dictionary<string, Region[]>(); // I guess init this here? i dunno
                landmasses.Add(region);
            }
        }

        // UwU
        //Debug.Log("vicky ur super cute :3333333 I luuuuuvs you!!!!");
        // Adds RegionNeighbors data to our large Regions
        for (int i = 0; i < landmasses.Count - 1; i++) // compare every landmass to every one in front of it in the List. 
        {                                                       // This compares everything with everything else, without duplicates.
            List<Region> regionNeighbors_n = new List<Region>();
            List<Region> regionNeighbors_e = new List<Region>();
            List<Region> regionNeighbors_s = new List<Region>();
            List<Region> regionNeighbors_w = new List<Region>();

            Region currentRegion = landmasses[i];

            int curr_w = currentRegion.Bounds[0].Item1;
            int curr_n = currentRegion.Bounds[0].Item2;
            int curr_e = currentRegion.Bounds[1].Item1;
            int curr_s = currentRegion.Bounds[1].Item2;


            for (int j = i + 1; j < landmasses.Count; j++)
            {
                Region compareToRegion = landmasses[j];

                int comp_w = compareToRegion.Bounds[0].Item1;
                int comp_n = compareToRegion.Bounds[0].Item2;
                int comp_e = compareToRegion.Bounds[1].Item1;
                int comp_s = compareToRegion.Bounds[1].Item2;


                if (curr_s <= comp_n && curr_n >= comp_s) // compareToRegion is on the east or west of current region
                {
                    if (curr_e < comp_e) regionNeighbors_e.Add(compareToRegion); //it's an east neighbor
                    if (curr_w > comp_e) regionNeighbors_w.Add(compareToRegion); // it's a west neighbor
                }

                if (curr_w <= comp_e && curr_e >= comp_w) // compareToRegion is on the north or south of current region
                {
                    if (curr_n < comp_n) regionNeighbors_n.Add(compareToRegion); //it's a north neighbor
                    if (curr_s > comp_s) regionNeighbors_s.Add(compareToRegion); // it's a south neighbor
                }
            }

            if (regionNeighbors_n != null) { currentRegion.RegionNeighbors.Add("n", regionNeighbors_n.ToArray()); } //oh we can just add these as we go
            if (regionNeighbors_e != null) { currentRegion.RegionNeighbors.Add("e", regionNeighbors_e.ToArray()); } // no reason to store and then set them
            if (regionNeighbors_s != null) { currentRegion.RegionNeighbors.Add("s", regionNeighbors_s.ToArray()); }
            if (regionNeighbors_w != null) { currentRegion.RegionNeighbors.Add("w", regionNeighbors_w.ToArray()); }
        }

        CreateMidlines(landmasses);
    }

    private void CreateMidlines(List<Region> largeRegions)
    {
        int counter = 1;
        foreach (Region curr_region in largeRegions) // for every region landmass
        {
            int curr_w = curr_region.Bounds[0].Item1;
            int curr_n = curr_region.Bounds[0].Item2;
            int curr_e = curr_region.Bounds[1].Item1;
            int curr_s = curr_region.Bounds[1].Item2;

            // init our neighbormidpoints dict for this Region
            curr_region.NeighborMidpoints = new Dictionary<string, (float, float)[]>();

            foreach (KeyValuePair<string, Region[]> neighbors in curr_region.RegionNeighbors)  // for every neighbor quadrant
            {
                string neighborDirection = neighbors.Key;
                Region[] regionNeighbors = neighbors.Value;


                //init a new List to store midpoints for this neighbor.
                List<(float x, float y)> midpoints = new List<(float x, float y)>();

                foreach (Region comp_region in regionNeighbors)                                   //for every Region object in that neighbor quadrant
                {
                    List<(Tile, Tile)> filteredCurrLines = new List<(Tile, Tile)>();
                    List<(Tile, Tile)> filteredCompLines = new List<(Tile, Tile)>();

                    List<(Tile start, Tile end)> AllCurrLines = new List<(Tile start, Tile end)>();
                    List<(Tile start, Tile end)> AllCompLines = new List<(Tile start, Tile end)>();


                    int comp_w = comp_region.Bounds[0].x;
                    int comp_n = comp_region.Bounds[0].y;
                    int comp_e = comp_region.Bounds[1].x;
                    int comp_s = comp_region.Bounds[1].y;
                    
                    // Set the row or column bounds we'll be iterating over
                    int upper_row_y = Mathf.Min(curr_n, comp_n);
                    int lower_row_y = Mathf.Max(curr_s, comp_s);
                    int upper_column_x = Mathf.Min(curr_e, comp_e);
                    int lower_column_x = Mathf.Max(curr_w, comp_w);

                    int iter_from = 0;
                    int iter_to = 0;

                    // setting values
                    if (neighborDirection == "n")
                    {
                        // Create a filtered list of Lines so we aren't iterating over all of them.
                        filteredCurrLines = FilterHullLines(curr_region, "n");
                        filteredCompLines = FilterHullLines(comp_region, "s");
                        iter_from = lower_column_x;
                        iter_to = upper_column_x;
                    }
                    if (neighborDirection == "s")
                    {
                        // Create a filtered list of Lines so we aren't iterating over all of them.
                        filteredCurrLines = FilterHullLines(curr_region, "s");
                        filteredCompLines = FilterHullLines(comp_region, "n");
                        iter_from = lower_column_x;
                        iter_to = upper_column_x;
                    }

                    if (neighborDirection == "e")
                    {
                        // Create a filtered list of Lines so we aren't iterating over all of them.
                        filteredCurrLines = FilterHullLines(curr_region, "e");
                        filteredCompLines = FilterHullLines(comp_region, "w");
                        iter_from = lower_row_y;
                        iter_to = upper_row_y;
                    }
                    if (neighborDirection == "w")
                    {
                        // Create a filtered list of Lines so we aren't iterating over all of them.
                        filteredCurrLines = FilterHullLines(curr_region, "w");
                        filteredCompLines = FilterHullLines(comp_region, "e");
                        iter_from = lower_row_y;
                        iter_to = upper_row_y;
                    }

                    for (int iter = iter_from; iter < iter_to + 1; iter++) // for every x or y in range of rows or columns we're looking for:
                    {   
                        // DEBUG
                        if (iter == iter_from) Debug.Log($"curr: {curr_region.colorName}  comp: {comp_region.colorName}   direction: {neighborDirection}");

                        if (neighborDirection == "e" || neighborDirection == "w")
                        {
                            AllCurrLines = GetHullLinesAtXorYIntersect(curr_region, row_y_intercept: iter, optionalListOfHullLines: filteredCurrLines);
                            AllCompLines = GetHullLinesAtXorYIntersect(comp_region, row_y_intercept: iter, optionalListOfHullLines: filteredCompLines);
                        }
                        if (neighborDirection == "n" || neighborDirection == "s")
                        {
                            AllCurrLines = GetHullLinesAtXorYIntersect(curr_region, column_x_intercept: iter, optionalListOfHullLines: filteredCurrLines);
                            AllCompLines = GetHullLinesAtXorYIntersect(comp_region, column_x_intercept: iter, optionalListOfHullLines: filteredCompLines);
                        }

                        // These are the two points we will be calculating for this row or column
                        // so that we can get their midpoint
                        // which is the whole point of all this lol
                        (float x, float y) point1 = (-1,-1);
                        (float x, float y) point2 = (-1, -1); 

                        // This gets the first point
                        if (AllCurrLines.Count == 1) // if we only found one line, calc the point1 coord.
                        {
                            (int, int) line1_x1y1 = (AllCurrLines[0].start.x, AllCurrLines[0].start.y);
                            (int, int) line1_x2y2 = (AllCurrLines[0].end.x, AllCurrLines[0].end.y);
                            
                            var pt = PointOnLine(line1_x1y1, line1_x2y2, iter);
                            if (pt != (0f, 0f)) 
                            {
                                point1 = pt;
                            }
                            else // slope of zero, get whichever point is closest to comp?
                            {
                                // for now just going to skip this row
                                continue;
                            }
                           
                        }
                        else if (AllCurrLines.Count == 2) // we returned two lines because we are at a vertex, so find the common point and set point1 to that
                        {
                            var line1 = AllCurrLines[0];
                            var line2 = AllCurrLines[1];
                            // if start of first line equals start/end of the second line, then start of first line is the coord we want.
                            if ((line1.start.x, line1.start.y) == (line2.start.x, line2.start.y) || (line1.start.x, line1.start.y) == (line2.end.x, line2.end.y))
                            {
                                point1 = ((float)line1.start.x, (float)line1.start.y);
                            }
                            else // otherwise it's the end of that line that shares the point
                            {
                                point1 = ((float)line1.end.x, (float)line1.end.y);
                            }

                        }

                        // This gets the second point
                        if (AllCompLines.Count == 1)
                        {
                            //Debug.Log($"allcomplines.count == {AllCompLines.Count}");
/*                            foreach ((Tile one,Tile two) line in AllCompLines)
                            {
                                Debug.Log($"got line ({(line.one.x,line.one.y)}) ({(line.two.x,line.two.y)})");
                            }*/

                            (int, int) line2_x1y1 = (AllCompLines[0].start.x, AllCompLines[0].start.y);
                            (int, int) line2_x2y2 = (AllCompLines[0].end.x, AllCompLines[0].end.y);

                            var pt = PointOnLine(line2_x1y1, line2_x2y2, iter);
                            if (pt != (0f, 0f))
                            {
                                point2 = pt;
                            }
                            else // slope of zero, get whichever point is closest to comp?
                            {
                                // for now just going to skip this row
                                continue;
                            }
                             
                        }
                        else if (AllCompLines.Count == 2)
                        {
                            var line1 = AllCompLines[0];
                            var line2 = AllCompLines[1];
                            // if start of first line equals start/end of the second line, then start of first line is the coord we want.
                            if ((line1.start.x, line1.start.y) == (line2.start.x, line2.start.y) || (line1.start.x, line1.start.y) == (line2.end.x, line2.end.y))
                            {
                                point2 = ((float)line1.start.x, (float)line1.start.y);
                            }
                            else // otherwise it's the end of that line that shares the point
                            {
                                point2 = ((float)line1.end.x, (float)line1.end.y);
                            }
                        }

                        // calculating the midpoint of the two found coords
                        if (point1 != (-1,-1) && point2 != (-1,-1))
                        {
                            (float, float) midpoint_coord = ((point1.x + point2.x) / 2, (point1.y + point2.y) / 2);

                            // Add the coord to our midpoints List.
                            midpoints.Add(midpoint_coord);
                        }
                        

                        //don't need this rn, but this measures dist between two points
                        //var distance = Mathf.Sqrt(Mathf.Pow(point2.x - point1.x, 2) + Mathf.Pow(point2.y - point1.y, 2));

                    }
                    // DEBUG
                    //Debug.Log($"found {midpoints.Count} for ^^^^");

                }

                // create our dict entry of midpoints for this neighbor using the finished midpoints list.
                curr_region.NeighborMidpoints.Add(neighborDirection, midpoints.ToArray());
                allmidpointsDEBUG.Add(midpoints);
                Debug.Log($"Region: {curr_region.colorName}, Direction: {neighborDirection}, Number of midpoints found: {midpoints.Count}");
                
                
            }
            //Debug.Log($"Got done with region {counter}");
            counter++;
            if (counter == 2) break; // so we only do the first one for debug.
        }


        /// <Summary>
        /// Returns a list of ALL lines that a given row OR column intercepts on a given Region obj.
        /// </Summary>
        List<(Tile, Tile)> GetHullLinesAtXorYIntersect(Region region, int row_y_intercept = -1, int column_x_intercept = -1, List<(Tile, Tile)> optionalListOfHullLines = null)
        {
            List<(Tile, Tile)> intersectedLines = new List<(Tile, Tile)>();

            if (row_y_intercept < 0 && column_x_intercept < 0)
            {
                Debug.Log("GetHullLineAtIntersect() requires one intercept parameter. Recieved none.");
                return intersectedLines;
            }
            if (row_y_intercept >= 0 && column_x_intercept >= 0)
            {
                Debug.Log("GetHullLineAtIntersect() requires one intercept parameter. Recieved two.");
                return intersectedLines;
            }

            var listOfTiles = region.HullLines;
            if (optionalListOfHullLines != null) listOfTiles = optionalListOfHullLines.ToArray();

            foreach ((Tile point1, Tile point2) hullLine in listOfTiles)
            {
                var x1 = hullLine.point1.x;
                var y1 = hullLine.point1.y;

                var x2 = hullLine.point2.x;
                var y2 = hullLine.point2.y;

                if (row_y_intercept >= 0)
                {
                    if (row_y_intercept <= Mathf.Max(y1, y2) && row_y_intercept >= Mathf.Min(y1, y2)) // if we are intersecting the middle of a line 
                    {
                        intersectedLines.Add(hullLine);
                    }
                }

                if (column_x_intercept >= 0)
                {
                    if (column_x_intercept <= Mathf.Max(x1, x2) && column_x_intercept >= Mathf.Min(x1, x2)) // if we are intersecting the middle of a line 
                    {
                        intersectedLines.Add(hullLine);
                    }
                }
            }
            return intersectedLines;
        }

        List<(Tile, Tile)> FilterHullLines(Region region, string direction_to_filter)
        {
            List<(Tile, Tile)> filteredLines = new List<(Tile, Tile)>();

            (int x, int y) n = (0, 0);
            (int x, int y) e = (0, 0);
            (int x, int y) s = (0, int.MaxValue);
            (int x, int y) w = (int.MaxValue, 0);

            // TODO: this is how the region.Bounds SHOULD be stored, idk wtf I was thinking before.
            for (int i = 0; i < region.HullPoints.Length; i++)
            {
                if (region.HullPoints[i].y > n.y) n = (region.HullPoints[i].x, region.HullPoints[i].y); // Get n bound (highest Y)
                if (region.HullPoints[i].x > e.x) e = (region.HullPoints[i].x, region.HullPoints[i].y); // Get e bound (highest x)
                if (region.HullPoints[i].y < s.y) s = (region.HullPoints[i].x, region.HullPoints[i].y); // Get s bound (lowest Y)
                if (region.HullPoints[i].x < w.x) w = (region.HullPoints[i].x, region.HullPoints[i].y); // Get w bound (lowest x)
            }

            if (direction_to_filter == "n")
            {
                for (int i = 0; i < region.HullLines.Length; i++)
                {
                    if (Mathf.Min(region.HullLines[i].Item1.y, region.HullLines[i].Item2.y) >= Mathf.Min(e.y, w.y))
                    {
                        filteredLines.Add(region.HullLines[i]);
                    }
                }
            }
            if (direction_to_filter == "e")
            {
                for (int i = 0; i < region.HullLines.Length; i++)
                {
                    if (Mathf.Min(region.HullLines[i].Item1.x, region.HullLines[i].Item2.x) >= Mathf.Min(n.x, s.x))
                    {
                        filteredLines.Add(region.HullLines[i]);
                    }
                }
            }
            if (direction_to_filter == "s")
            {
                for (int i = 0; i < region.HullLines.Length; i++)
                {
                    if (Mathf.Max(region.HullLines[i].Item1.y, region.HullLines[i].Item2.y) <= Mathf.Min(e.y, w.y))
                    {
                        filteredLines.Add(region.HullLines[i]);
                    }
                }
            }
            if (direction_to_filter == "w")
            {
                for (int i = 0; i < region.HullLines.Length; i++)
                {
                    if (Mathf.Max(region.HullLines[i].Item1.x, region.HullLines[i].Item2.x) <= Mathf.Max(n.x, s.x))
                    {
                        filteredLines.Add(region.HullLines[i]);
                    }
                }
            }

            return filteredLines;
        }

        (float x, float y) PointOnLine((int x, int y) start, (int x, int y) end, int given_Y)
        {   
            

            var x1 = start.x;
            var y1 = start.y;
            var x2 = end.x;
            var y2 = end.y;

            if (y2 - y1 == 0 || x2 - x1 == 0)
            {   // if it's a horizontal line then slope fails
                //Debug.Log("div by zero trying to get point on line uhh  handle for this");
                return (0f, 0f);
            }

            float slope = (y2 - y1) / (x2 - x1);
            var X_for_coord_along_line = ((given_Y - y1) / slope) + x1;
            var point_along_line = (X_for_coord_along_line, given_Y);

            return point_along_line;
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
        //Debug.Log(entryPoints.Count);
        for (int i = 0; i < entryPoints.Count - 1; i++) //stop at second-to-last
        {
            for (int j = i + 1; j < (entryPoints.Count); j++)
            {
                // if x or y is on the same border side, go to next
                (int x, int y) end = entryPoints[j];
                if (entryPoints[i].x == end.x) continue;
                if (entryPoints[i].y == end.y) continue;


                Tile startT = map.GetTile(entryPoints[i].x, entryPoints[i].y);
                Tile endT = map.GetTile(end.x, end.y);
                List<Tile> path = pathFinding.new_Astar(startT, endT, elevationLimitForPathfind);

                if (path != null)
                {
                    //Debug.Log($"Path from {entryPoints[i]} to {end} has length of {path.Count}");
                    paths.Add(path);
                }
                // else Debug.Log($"No path for {entryPoints[i]} to {end} !!!!!!");
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

        if (showMidpoints)
        {
            
            if (allmidpointsDEBUG != null)
            {

                foreach (List<(float,float)> midline in allmidpointsDEBUG)
                {
                    Vector3[] arrr = new Vector3[midline.Count];

                    var i = 0;
                    foreach ((float,float) xy in midline)
                    {
                        arrr[i] = new Vector3(xy.Item1, 5, xy.Item2);
                        
                    }
                    
                    Gizmos.color = Color.blue;
                    Gizmos.DrawLineStrip(arrr, false);
                }
            }
        }

    }

    public void runMapGen() //this is so the Editor script can auto update on changes
    {
        lt.gameManager.loadNewData();
    }





}


