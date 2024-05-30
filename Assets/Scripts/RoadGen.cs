using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadGen : MonoBehaviour
{
    [SerializeField] LayerTerrain lt;
    [SerializeField] Color roadColor;


    private float[,] noiseMap;
    private Color[] colorMap;
    private float[,] roadMapData;
    private List<(int, int)> entryPoints = new List<(int, int)>();

    public int entryGapMin = 6;



    /* 
       1) Find map entries: loop around exterior of terrain.Find all of the low elevation levels.
             The goal is to find the longest distance from these points, with the least amount of curve.
       2) Pathfind between each point using only flat terrain. (for now)


     */

    public Color[] GetArterialPaths(float[,] noisyMcNoiseFace, Color[] colorMcMapFace)
    {
        noiseMap = noisyMcNoiseFace;
        colorMap = colorMcMapFace;

        colorMap = GetMapEntries();

        return colorMap;
    }

    private Color[] GetMapEntries()
    {
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);

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

        for (int row = 1; row < height; row++)
        {   
            // bottom-right --> top right
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

        for (int col = width - 1; col <= 0; col--)
        {
            // top-right --> top left
            if (noiseMap[height - 1, col] < 0.01f)
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


        for (int row = height-1; row <= 0; row--)
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

        foreach ((int, int) coord in entryPoints)
        {
            //Debug.Log(coord);
            (int colx, int rowy) = coord;

            colorMap[colx * width + rowy] = Color.red;
        }

        return colorMap;
    }

    public void ApplyTexture(float[,] noiseMap)
    {

    }


}
