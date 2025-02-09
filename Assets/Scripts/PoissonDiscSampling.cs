using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PoissonDiscSampling {


	public static List<Vector2> GeneratePoints(float minRadius, float maxRadius, Vector2 sampleRegionSize, int numSamplesBeforeRejection = 30) {

		// radius is diagonal of cell, we need the size of each edge of the square cell
		float cellSize = minRadius/Mathf.Sqrt(2);

		// make array of ints (zeros) in size of sampleRegion. so if cell is 2 and region is 300, it would be 150x150 array 
		int[,] grid = new int[Mathf.CeilToInt(sampleRegionSize.x/cellSize), Mathf.CeilToInt(sampleRegionSize.y/cellSize)];

		
		List<Vector2> points = new List<Vector2>();
		List<Vector2> spawnPoints = new List<Vector2>(); //when added to points, added to spawnpoints, if fails removes spanpoint

		spawnPoints.Add(sampleRegionSize/2); //add random spawnpoint to start at

		while (spawnPoints.Count > 0) {

			//get random spawnCentre from spawnPoints
			int spawnIndex = Random.Range(0,spawnPoints.Count);
			Vector2 spawnCentre = spawnPoints[spawnIndex];

			bool candidateAccepted = false;

			// get random angle, see if it's valid, if is add to spawnpoints
			for (int i = 0; i < numSamplesBeforeRejection; i++)
			{	
				// gets a random angle and direction to find a new point on
				float angle = Random.value * Mathf.PI * 2;
				Vector2 dir = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));

				// TODO: Seb has it as maxed at 2x the radius. 
				//We want to use the MaxRadius instead, looking it up in the underlying perlin noise
				Vector2 candidate = spawnCentre + dir * Random.Range(minRadius, maxRadius);

				if (IsValid(candidate, sampleRegionSize, cellSize, minRadius, maxRadius, points, grid)) 
				{
					points.Add(candidate);
					spawnPoints.Add(candidate);
					grid[(int)(candidate.x/cellSize),(int)(candidate.y/cellSize)] = points.Count;
					candidateAccepted = true;
					break;
				}
			}
			if (!candidateAccepted) {
				spawnPoints.RemoveAt(spawnIndex);
			}

		}

		return points;
	}

	public static List<Vector2> GeneratePointsPerlin(float minRadius, float maxRadius, Vector2 sampleRegionSize, int numSamplesBeforeRejection = 30, float[] perlinNoise = null)
	{	

		/*
		use the maximum radius for the cell size, not min
		run perlin get noise map
		check distance against value of underlying for the radius at each stop

		 
		 
		 */
		float cellSize = maxRadius / Mathf.Sqrt(2);

		int[,] grid = new int[Mathf.CeilToInt(sampleRegionSize.x / cellSize), Mathf.CeilToInt(sampleRegionSize.y / cellSize)];
		List<Vector2> points = new List<Vector2>();
		List<Vector2> spawnPoints = new List<Vector2>();

		spawnPoints.Add(sampleRegionSize / 2);
		while (spawnPoints.Count > 0)
		{
			int spawnIndex = Random.Range(0, spawnPoints.Count);
			Vector2 spawnCentre = spawnPoints[spawnIndex];
			bool candidateAccepted = false;

			for (int i = 0; i < numSamplesBeforeRejection; i++)
			{
				float angle = Random.value * Mathf.PI * 2;
				Vector2 dir = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
				Vector2 candidate = spawnCentre + dir * Random.Range(minRadius, maxRadius);
				if (IsValid(candidate, sampleRegionSize, cellSize, minRadius, maxRadius, points, grid))
				{
					points.Add(candidate);
					spawnPoints.Add(candidate);
					grid[(int)(candidate.x / cellSize), (int)(candidate.y / cellSize)] = points.Count;
					candidateAccepted = true;
					break;
				}
			}
			if (!candidateAccepted)
			{
				spawnPoints.RemoveAt(spawnIndex);
			}

		}

		return points;
	}


	// checks the surrounding cells around a candidate
	// to make sure there aren't any too close to it which would invalidate it
	static bool IsValid(Vector2 candidate, Vector2 sampleRegionSize, float cellSize, float minRadius, float maxRadius, List<Vector2> points, int[,] grid) {
		
		// check if candidate is within the sample region on the map
		if (candidate.x >=0 && candidate.x < sampleRegionSize.x && candidate.y >= 0 && candidate.y < sampleRegionSize.y) 
		{	
			
			// find which cell the candidate point lies in	so we can search surrounding cells		
			int cellX = (int)(candidate.x/cellSize);
			int cellY = (int)(candidate.y/cellSize);

			// this gets the bounds of the area we are searching.
			// the min/max is to make sure points aren't off map
			// Seb has it simply as -2, we would want that to be based on the Perlin noise underlying
			int searchStartX = Mathf.Max(0,cellX -2);
			int searchEndX = Mathf.Min(cellX+2,grid.GetLength(0)-1);
			int searchStartY = Mathf.Max(0,cellY -2);
			int searchEndY = Mathf.Min(cellY+2,grid.GetLength(1)-1);

			for (int x = searchStartX; x <= searchEndX; x++) {
				for (int y = searchStartY; y <= searchEndY; y++) {

					// if pointIndex = -1 there is NO point in that spot
					int pointIndex = grid[x,y]-1;

					// so if there is a point, check its distance
					if (pointIndex != -1) {
						float sqrDst = (candidate - points[pointIndex]).sqrMagnitude; //sqrMag is less expensive than Magnitude
						if (sqrDst < maxRadius*maxRadius) {
							return false;
						}
					}
				}
			}
			return true;
		}
		return false; //return false if point not on map
	}
}
