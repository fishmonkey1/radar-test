using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProcGenTiles;


public class RoadGen : MonoBehaviour
{
    [SerializeField] LayerTerrain lt;
    [SerializeField] Color roadColor;
    public Pathfinding pathFinding;


    private float[,] noiseMap;
    private Color[] colorMap;
    private float[,] roadMapData;
    private List<(int, int)> entryPoints = new List<(int, int)>();
    List<List<(int x, int xy)>> paths;
    int width;
    int height;

    public int entryGapMin = 6;
    public float elevationLimitForPathfind = 0.01f;
    





    /* 
       1) Find map entries: loop around exterior of terrain. Find all of the low-elevation levels.
               The goal is to find the longest distance from these points to another point, with the least amount of curve.
               This is the arterial road(s). Think highways going through a city...
        
       2) Pathfind between each point using only flat terrain (for now) to keep things simple.
               Need to pathfind start/end each found entry point.
                    For each node along path, run a normal to the nearest opposite landmass (if something close within X meters).
                    The pathfinding will follow the edge of a mountain, which is fine, but if it's going between two landmasses we want it centered between the two.
               Find one or two roads that are the longest and most

       


     */

    public Color[] GetArterialPaths(float[,] noisyMcNoiseFace, Color[] colorMcMapFace)
    {
        Debug.Log("=============== RoadGen =======================");
        // Finding path from (0, 34) to (0, 95)

        noiseMap = noisyMcNoiseFace;
        colorMap = colorMcMapFace;
        width = noiseMap.GetLength(0);
        height = noiseMap.GetLength(1);
        pathFinding = new Pathfinding(lt.finalMap); // this was being created after tryying to use it lol

        entryPoints = GetMapEntries();

        //entryPoints.Add((0,95));
        //entryPoints.Add((146, 255));

        Debug.Log($"found {entryPoints.Count} entryPoints");
        foreach ((int x, int y) points in entryPoints)
        {
            Debug.Log($"({points.Item1},{points.Item2})");
        }

        paths = PathfindEachEntry(entryPoints);
        Debug.Log($"PathfindEachEntry() returned: {paths} , which contains {paths.Count} paths");
        
        DrawPathsOnColorMap(paths);



        return colorMap;
    }

    /// <summary>
    /// Returns a List<(int x, int y)> of entry points into the map.
    /// </summary>
    private List<(int, int)> GetMapEntries()
    {
        roadMapData = new float[width, height];
        
        int lengthBottomSegment = 0;
        int lengthTopSegment = 0;
        int lengthLeftSegment = 0;
        int lengthRightSegment = 0;

        // Loop around edges of noiseMap, find low points, add to roadMapData
        // bottom left (0,0) --> bottom right (0,length)
        for (int col = 0; col < width; col++)
        {
            if (noiseMap[0, col] < 0.01f)
            {
                roadMapData[0, col] = 1f;
                colorMap[0 * width + col] = roadColor; //works
                lengthBottomSegment += 1;
            }
            else
            {
                if (lengthBottomSegment > 0)
                {
                    if (lengthBottomSegment >= entryGapMin)
                    {
                        int pointY = col - (lengthBottomSegment / 2);
                        entryPoints.Add((0, pointY));
                        lengthBottomSegment = 0;
                    }
                }
            }
        }

        for (int row = 1; row < height-1; row++)
        {
            // bottom-right (0,length) --> top right (height, length)
            if (noiseMap[row, width - 1] <= 0.01f)
            {
                roadMapData[row, width - 1] = 1f;
                colorMap[row * width - 1] = roadColor;
                lengthRightSegment += 1;
            }
            else
            {
                if (lengthRightSegment > 0)
                {
                    if (lengthRightSegment >= entryGapMin)
                    {
                        int pointY = row - (lengthRightSegment / 2);
                        entryPoints.Add((pointY, width - 1));
                        lengthRightSegment = 0;
                    }
                }
            }

        }

        
        for (int col = width-1; col > 0; col--)
        {
            // top-right (height, length) --> top left (height,0)
            if (noiseMap[height - 1, col] <= 0.01f)
            {
                roadMapData[height - 1, col] = 1f;
                colorMap[(width - 1) * width + col] = roadColor; //works
                lengthTopSegment += 1;
            }
            else
            {
                if (lengthTopSegment > 0)
                {
                    if (lengthTopSegment >= entryGapMin)
                    {
                        int pointY = col - (lengthTopSegment / 2);
                        entryPoints.Add((width - 1, pointY));
                        lengthTopSegment = 0;
                    }
                }
            }
        }


        for (int row = height - 1; row > 0; row--)
        {
            // top-left to bottom-right (start)
            if (noiseMap[row, 0] <= 0.01f)
            {
                roadMapData[row, 0] = 1f;
                colorMap[row * width] = roadColor;
                lengthLeftSegment += 1;
            }
            else
            {
                if (lengthLeftSegment > 0)
                {
                    if (lengthLeftSegment >= entryGapMin)
                    {
                        int pointY = row - (lengthLeftSegment / 2);
                        entryPoints.Add((pointY, 0));
                        lengthLeftSegment = 0;
                    }
                }
            }
        }

        foreach ((int x, int y) points in entryPoints)
        {
            //Debug.Log($"applying path color at ({points.Item1},{points.Item2})");
            ApplyRoadAtPoint(points.Item1, points.Item2);
        }

        return entryPoints;
    }

    /// <summary>
    /// Returns List<List<(int x, int xy)>> of pathfinds into the map.
    /// </summary>
    private List<List<(int x, int xy)>> PathfindEachEntry(List<(int x, int y)> entryPoints)
    {
        paths = new List<List<(int x, int xy)>>();

        for (int i = 0; i < entryPoints.Count-1; i++)
        {
            List<(int x, int y)> foundPath = new List<(int x, int y)>();

            (int x, int y) xy_end;
            
            xy_end = entryPoints[i + 1];
            Debug.Log($"Finding path from {entryPoints[i]} to {xy_end}");
            foundPath = pathFinding.AStar(entryPoints[i], xy_end, noiseMap, elevationLimitForPathfind);
            Debug.Log($"pathfinding found path with length: {foundPath.Count}");

            paths.Add(foundPath);
        }
        return paths;
    }



    private void DrawPathsOnColorMap(List<List<(int x, int xy)>> listOfPaths)
    {
        foreach (List<(int x, int xy)> path in listOfPaths)
        {
            foreach ((int x, int y) points in path)
            {
                //Debug.Log($"applying path color at ({points.Item1},{points.Item2})");
                ApplyRoadAtPoint(points.Item1, points.Item2);
            }
        }
    }



    public void ApplyRoadAtPoint(int x, int y)
    {
        colorMap[x * width + y] = Color.red;
    }


}
