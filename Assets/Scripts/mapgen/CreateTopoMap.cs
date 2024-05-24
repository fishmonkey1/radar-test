using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateTopoMap : MonoBehaviour
{
    [SerializeField]private GameObject topoObject;

    private List<Texture2D> texturesList;
    private Dictionary<Texture2D, int> topoTexturesDict;
    private TerrainData terrainData;
    [SerializeField] private LayerTerrain lt;
    private int start_x, start_y, end_x, end_y;

    private bool debug = false;
    public void createTopoTextures(int start_x, int start_y, int end_x, int end_y, bool deform)
    {   
        
        terrainData = lt.terrain.terrainData;
        topoObject = lt.topoObject;

        float bandDistance = lt.depth / lt.numberOfTopoLevels; //size of bands to create
        
        List<float> bands = new List<float>();

        float lerpStep = 1f / (float)lt.numberOfTopoLevels;
        float currLerp = 0f;

        List<Color> colors = new List<Color>();
        Texture2D texture = new Texture2D(lt.X, lt.Y);
        Renderer rend = topoObject.GetComponent<Renderer>(); 

        Color[] colorMap = new Color[lt.X * lt.Y];

        //make list of all the colors
        for (int i = 0; i < lt.numberOfTopoLevels; i++)
        {
            var tileColor = Color.Lerp(lt.topoColor1, lt.topoColor2, currLerp);
            tileColor.a = .5f;
            colors.Add(tileColor);
            currLerp += lerpStep;
        }
        if (debug) Debug.Log("-----colors--------------------------------------------");
        foreach (Color x in colors) if (debug) Debug.Log(x.ToString());

        if (debug) Debug.Log("-----band values--------------------------------------------");
        // make list of bands
        //this is working
        float val = 0;
        while (val < lt.depth)
        {
            val += bandDistance;
            float lerpedval = Mathf.InverseLerp(0f, (float)lt.depth, val);
            bands.Add(lerpedval);
        }

        foreach (float x in bands) if (debug) Debug.Log(x.ToString());
        if (debug) Debug.Log("---------------------------------------------------------");
         if (debug) Debug.Log("colors list: "+ colors.Count);
         if (debug) Debug.Log("bands list: " + bands.Count);


        List<string> cm = new List<string>();
        int count = 0;
        // Creating colormap
        for (int y = 0; y < lt.Y; y++)
        {
            for (int x = 0; x < lt.X; x++)
            {
                float elevation = lt.finalMap.GetTile(x, y).ValuesHere[LayersEnum.Elevation];
                
                for (int j = 0; j < bands.Count; j++)
                {
                    if (elevation < bands[j])
                    {
                        colorMap[x * lt.X + y] = colors[j];

                        if (!cm.Contains(colorMap[count].ToString()))
                        {
                            cm.Add(colorMap[y].ToString());
                        }
                        
                        break; //dont need to check rest of bands
                    }
                }
            }
        }
        if (debug) Debug.Log("-----unique colors in colorMap before setting pixels---------");
        if (debug) foreach (string x in cm) Debug.Log(x);
        if (debug) Debug.Log("-----unique colors in texture before setting pixels---------");

        List<string> good1 = new List<string>();
        foreach (Color pixel in texture.GetPixels())
            if (!good1.Contains(pixel.ToString()))
            {
                good1.Add(pixel.ToString());
            }
        if (debug) foreach (string x in good1) Debug.Log(x);
        if (debug) Debug.Log("--------------------------------------------------------------");
        if (debug) Debug.Log("Size of GetPixels()[] : "+texture.GetPixels().Length);
        if (debug) Debug.Log("Size of colorMap[] : " + colorMap.Length);
        if (debug) Debug.Log("colorMap[] is a : " + colorMap.GetType());


        texture.SetPixels(colorMap);
        texture.Apply();
        rend.material.mainTexture = texture;
    }

}
