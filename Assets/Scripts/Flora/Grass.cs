using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*
 create noisemap
    use step function to make 1or0 areas
    visualize over terrain for now
    PSD but for any no-grass area, don't spawn grass
 
 
 */



public class Grass
{

    private Flora flora;

    // Width and height of the texture in pixels.
    public int pixWidth;
    public int pixHeight;

    // The origin of the sampled area in the plane.
    public float xOrg = 0f;
    public float yOrg = 0f;

    // The number of cycles of the basic noise pattern that are repeated
    // over the width and height of the texture.
    [SerializeField] public float scale = 1.0F;
    [SerializeField] [Range(0f,1f)] public float step = .5f;

    [SerializeField] Color sand; //CBBD93
    [SerializeField] Color notsand; 

    private Texture2D noiseTex;
    private Color[] pix;
    private Renderer rend;

    void Start()
    {
        
    }

    public void CalcNoise()
    {
        notsand = Color.green;

        pixWidth = (int)flora.sampleRegionSize.x;
        pixHeight = (int)flora.sampleRegionSize.y;

        rend = flora.terrainPlaneObj.GetComponent<Renderer>();

        // Set up the texture and a Color array to hold pixels during processing.
        noiseTex = new Texture2D(pixWidth, pixHeight);
        pix = new Color[noiseTex.width * noiseTex.height];
        rend.sharedMaterial.mainTexture = noiseTex;

        // For each pixel in the texture...
        for (float y = 0.0F; y < noiseTex.height; y++)
        {
            for (float x = 0.0F; x < noiseTex.width; x++)
            {
                float xCoord = xOrg + x / noiseTex.width * scale;
                float yCoord = yOrg + y / noiseTex.height * scale;
                float sample = Mathf.PerlinNoise(xCoord, yCoord);
                pix[(int)y * noiseTex.width + (int)x] = new Color(sample, sample, sample);
            }
        }

        // Copy the pixel data to the texture and load it into the GPU.
        noiseTex.SetPixels(pix);
        noiseTex.Apply();
    }



}
