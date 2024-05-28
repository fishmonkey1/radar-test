using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResearchMapGenerator : MonoBehaviour
{
	/*public int mapWidth;
	public int mapHeight;
	public float noiseScale;*/

	

	[SerializeField] private LayerTerrain lt;
	public bool autoUpdate;


	public void GenerateMap()
	{	
		if (lt.DrawInEditor)
        {
			Debug.Log("got to ResearchMapGenerator.GenerateMap");
			//float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, noiseScale);
			lt.GenerateTerrain();
			float[,] noiseMap = lt.finalMap.FetchFloatValues(LayersEnum.Elevation);

			ResearchMapDisplay display = FindObjectOfType<ResearchMapDisplay>();
			display.DrawNoiseMap(noiseMap);
		}
	}
}
