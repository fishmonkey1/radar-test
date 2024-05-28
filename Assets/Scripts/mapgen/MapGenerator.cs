using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
	public enum DrawMode
    {
		NoiseMap,
		ColorMap,
		TopoMap,
		Mesh
    }

	public DrawMode drawMode;
	public TerrainType[] regions;

	[SerializeField] private LayerTerrain lt;



	public void GenerateMap()
	{	
		if (lt.DrawInEditor)
        {
			Debug.Log("got to MapGenerator.GenerateMap");
			lt.GenerateTerrain();
			float[,] noiseMap = lt.finalMap.FetchFloatValues(LayersEnum.Elevation);

			Color[] colorMap = new Color[lt.X * lt.Y];
			for (int y = 0; y < lt.Y; y++)
			{
				for (int x = 0; x < lt.X; x++)
				{
					float elevation = lt.finalMap.GetTile(x, y).ValuesHere[LayersEnum.Elevation];
					for (int j = 0; j < regions.Length; j++)
					{
						if (elevation <= regions[j].height)
						{
							colorMap[x * lt.X + y] = regions[j].color;
							break; //dont need to check rest of bands
						}
					}
				}
			}


			MapDisplay display = FindObjectOfType<MapDisplay>();
			if (drawMode == DrawMode.NoiseMap)
            {
				display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
			}
			else if (drawMode == DrawMode.ColorMap)
            {	
				display.DrawTexture(TextureGenerator.TextureFromColorMap(colorMap,lt.X, lt.Y));
			}
			else if (drawMode == DrawMode.Mesh)
			{
				display.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap), TextureGenerator.TextureFromColorMap(colorMap, lt.X, lt.Y));
			}
		}
	}
} 

[System.Serializable]
public struct TerrainType
{
	public string name;
	public float height;
	public Color color;
}
