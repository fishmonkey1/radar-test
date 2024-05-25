using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Text;

public class CreateTopoMap : MonoBehaviour
{
    [SerializeField] private GameObject topoObject;
    [SerializeField] private LayerTerrain lt;
    private bool makeTerrainTopographic;
    Texture2D texture;

    public void createTopoTextures(int start_x, int start_y, int end_x, int end_y, bool deform)
    {
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
        Texture2D[] texture2DMap = new Texture2D[lt.X * lt.Y];
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
        // creating the Texture2D from the colormap
        // applying it to the Topo plane
        Renderer rend = topoObject.GetComponent<Renderer>();
        texture.SetPixels(colorMap); //TODO: change to SetPixels32, it's faster. Especially if doing live reload
        texture.Apply();
        rend.material.mainTexture = texture;
        

        //--------------------------------------------------------------
        // save .png of topo 
        byte[] bytes = texture.EncodeToPNG();
        //var dirPath = Application.dataPath + "/../SaveImages/";
        string png_dir = "Assets/Textures_and_Models/Resources/TerrainTextures/topo/png";
        if (!Directory.Exists(png_dir))
        {
            Directory.CreateDirectory(png_dir);
        }
        else
        {
            // delete folder and re-add to del already existing texture
            Directory.Delete(png_dir, true);
            Directory.CreateDirectory(png_dir);
        }
        // saves the png file
        File.WriteAllBytes(png_dir + "Image" + ".png", bytes);
        
        //-------------------------------------
        // Create new TerrainLayer to override current TerrainLayers
        if (makeTerrainTopographic)
        {
            string tl_dir = "Assets/Textures_and_Models/Resources/TerrainTextures/topo/layers/Topographic.terrainlayer";

            TerrainLayer old_tl = AssetDatabase.LoadAssetAtPath<TerrainLayer>(tl_dir + "/Topographic.terrainlayer");
            TerrainLayer new_tl = new TerrainLayer();
            if (!old_tl) // create if none
            {
                Debug.Log("No file found at '"+tl_dir+"'. Creating new TerrainLayer");
                new_tl.diffuseTexture = texture;
                new_tl.tileSize = new Vector2(lt.X, lt.Y); //set tile size so it doesn't tile
                AssetDatabase.CreateAsset(new_tl, tl_dir);
                AssetDatabase.Refresh();
            }
            else //otherwise edit existing
            {
                Debug.Log("old_tl: "+old_tl);
                old_tl.diffuseTexture = texture;
                old_tl.tileSize = new Vector2(lt.X, lt.Y);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }
    }

    private void setSizeandLoc()
    {
        transform.position = new Vector3(lt.X/2, -2, lt.Y/2);
        transform.localScale = new Vector3((float)lt.X * .1f, 1, (float)lt.Y * .1f);
    }

}





         
 
