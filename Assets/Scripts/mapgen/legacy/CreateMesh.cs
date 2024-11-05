using UnityEngine;
using System.Collections;

public static class MeshGenerator
{
	public static MeshData GenerateTerrainMesh(float[,] heightMap, int depth, AnimationCurve meshHeightCurve)
	{
		int width = heightMap.GetLength(0);
		int height = heightMap.GetLength(1);


		MeshData meshData = new MeshData(width, height);
		int vertexIndex = 0;

		for (int x = 0; x < width; x++)
		{
			for (int z = 0; z < height; z++)
			{

				//meshData.vertices[vertexIndex] = new Vector3(x, heightMap[x, z] * depth, z);
				meshData.vertices[vertexIndex] = new Vector3(x, meshHeightCurve.Evaluate(heightMap[x, z]) * depth, z); //utilizes AnimationCurve 
				meshData.uvs[vertexIndex] = new Vector2(x / (float)width, z / (float)height);

				if (x < width - 1 && z < height - 1)
				{
					meshData.AddTriangle(vertexIndex, vertexIndex + width + 1, vertexIndex + width);
					meshData.AddTriangle(vertexIndex + width + 1, vertexIndex, vertexIndex + 1);
				}

				vertexIndex++;
			}
		}

		return meshData;

	}
}

public class MeshData
{
	public Vector3[] vertices;
	public int[] triangles;
	public Vector2[] uvs;

	int triangleIndex;

	public MeshData(int meshWidth, int meshHeight)
	{
		vertices = new Vector3[meshWidth * meshHeight];
		uvs = new Vector2[meshWidth * meshHeight];
		triangles = new int[(meshWidth - 1) * (meshHeight - 1) * 6];
	}

	public void AddTriangle(int a, int b, int c)
	{
		triangles[triangleIndex] = a;
		triangles[triangleIndex + 1] = b;
		triangles[triangleIndex + 2] = c;
		triangleIndex += 3;
	}

	public Mesh CreateMesh(bool mesh32bitBuffer)
	{
		Mesh mesh = new Mesh();

		// The default index format is 16 bit because it uses less memory and bandwidth.
		// if map larger than 256x256 it will use larger buffer.
		if (mesh32bitBuffer) 
		{ 
			mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; 
		}

		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.uv = uvs;
		mesh.RecalculateNormals();
		return mesh;
	}

}