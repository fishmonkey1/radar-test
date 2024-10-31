using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoisemapMeshGenerator : MonoBehaviour
{
    /*
     * run lt make 
     
     */
    [SerializeField] private LayerTerrain layerTerrain;

    public float[,] noiseMap;
    public float[,] noiseMap_ReversedYXarray;
    Color[] colorMap;

    // Start is called before the first frame update
    void Start()
    {
        //loadNewNoiseData();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void loadNewNoiseData()
    {
        layerTerrain.GenerateTerrain(); // runs all of layerTerrain's stuff, new noiseMap

        noiseMap = layerTerrain.finalMap.FetchFloatValues(LayersEnum.Elevation);
        noiseMap_ReversedYXarray = layerTerrain.finalMap.FetchFloatValues_ReversedYXarray(LayersEnum.Elevation); //store reversed 

        layerTerrain.genTopo.createTopoTextures(0, 0, layerTerrain.X, layerTerrain.Y, false, noiseMap);

    }

}
