using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResearchMapDisplay : MonoBehaviour
{
	[SerializeField] public Renderer textureRender;
	[SerializeField] public Color Color1;
	[SerializeField] public Color Color2;

	public void DrawNoiseMap(float[,] noiseMap)
	{	
		int width = noiseMap.GetLength(0);
		int height = noiseMap.GetLength(1);
		Debug.Log("Got to ResearchMapDisplay.DrawNoiseMap");
		Debug.Log("Width of noisemap is: "+ width);

		Texture2D texture = new Texture2D(width, height);

		Color[] colourMap = new Color[width * height];
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				colourMap[y * width + x] = Color.Lerp(Color1, Color2, noiseMap[x, y]);
			}
		}
		texture.SetPixels(colourMap);
		texture.Apply();
		
		 //Can't use textureRenderer.material becaause that is only instantiated at runtime
		 //must use sharedMaterial
		 

		textureRender.sharedMaterial.mainTexture = texture;
		textureRender.transform.localScale = new Vector3(width, 1, height);
	}
}
