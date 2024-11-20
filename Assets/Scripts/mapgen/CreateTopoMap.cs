using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Text;

public class CreateTopoMap : MonoBehaviour
{
    [SerializeField] private bool print_debug = false;
    [SerializeField] private GameObject topoObject;
    [SerializeField] private LayerTerrain lt;
    private bool makeTerrainTopographic;
    Texture2D texture;

    public void CreateTopoTextures(int start_x, int start_y, int end_x, int end_y, bool deform, float[,] noiseMap)
    {
        if (print_debug) Debug.Log("Running Topo Stuff");

        makeTerrainTopographic = lt.makeTerrainTextureTopo;
        texture = new Texture2D(lt.X, lt.Y);
        setSizeandLoc();

        // make list of all the colors
        List<Color> colors = new List<Color>();
        float currLerp = 0f;
        float lerpStep = 1f / (float)lt.numberOfTopoLevels;
        for (int i = 0; i < lt.numberOfTopoLevels; i++)
        {
            var tileColor = Color.Lerp(lt.topoColor1, lt.topoColor2, currLerp);
            tileColor.a = .5f;
            colors.Add(tileColor);
            currLerp += lerpStep;
        }
        if (print_debug) Debug.Log($"Created {colors.Count} colors");

        // make list of bands
        List<float> bands = new List<float>();
        float bandDistance = lt.depth / lt.numberOfTopoLevels;
        float val = 0;
        while (val < lt.depth)
        {
            val += bandDistance;
            float lerpedval = Mathf.InverseLerp(0f, (float)lt.depth, val);
            bands.Add(lerpedval);
        }
        if (print_debug) Debug.Log($"Created {bands.Count} bands");

        // Creating colormap
        Color[] colorMap = new Color[lt.X * lt.Y];

        List<int> usedColors = new List<int>();

        for (int y = 0; y < lt.Y; y++)
        {
            for (int x = 0; x < lt.X; x++)
            {
                float elevation = lt.finalMap.GetTile(x, y).ValuesHere[LayersEnum.Elevation];
                for (int j = 0; j < bands.Count; j++)
                {
                    if (elevation < bands[j])
                    {
                        // TODO: This fails if lt.numberOfTopoLevels != 10. Definitely rounding something wrong
                        if (!usedColors.Contains(j)) usedColors.Add(j);

                        colorMap[x * lt.X + y] = colors[j];
                        break; //dont need to check rest of bands
                    }
                }
            }
        }
        if (print_debug) Debug.Log($"Created colorMap with {usedColors.Count} colors");


        Renderer rend = topoObject.GetComponent<Renderer>();
        texture.SetPixels(colorMap); //TODO: change to SetPixels32, it's faster. Especially if doing live reload
        texture.Apply();
        rend.sharedMaterial.mainTexture = texture; //cuz doing shit in editor too

        if (print_debug) Debug.Log($"The created Texture2d texture is : {texture}");
        
    }

    private void setSizeandLoc()
    {
        transform.position = new Vector3(lt.X/2, -2, lt.Y/2);
        transform.localScale = new Vector3((float)lt.X * .1f, 1, (float)lt.Y * .1f);
    }

}





         
 
