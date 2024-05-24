using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateTopoMap : MonoBehaviour
{
    [SerializeField] private GameObject topoObject;
    [SerializeField] private LayerTerrain lt;

    private bool debug = false;
    public void createTopoTextures(int start_x, int start_y, int end_x, int end_y, bool deform)
    {   
        
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

        // Creating colormap
        Color[] colorMap = new Color[lt.X * lt.Y];
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
                        break; //dont need to check rest of bands
                    }
                }
            }
        }

        // if this starts to break,
        // make sure plane has existing material
        Texture2D texture = new Texture2D(lt.X, lt.Y);
        Renderer rend = topoObject.GetComponent<Renderer>();
        texture.SetPixels(colorMap); //TODO: change to SetPixels32, it's faster. Especially if doing live reload
        texture.Apply();
        rend.material.mainTexture = texture;
    }

}
