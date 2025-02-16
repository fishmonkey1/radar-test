using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PoissonDiscSampling {



	public static List<Vector2> GeneratePoints(float minRadius, float maxRadius, Vector2 sampleRegionSize, int numSamplesBeforeRejection = 30,  GameObject planeObj = null, List<Zone> zones = null) {

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

				// figure max radius based on underlying point
				//Vector2 candidate = calculateCandidate(spawnCentre);

				//(float, float) currentRadiusValues = GetZoneRadius(spawnCentre);
				Zone currZone = GetZone(spawnCentre);
				float currentMinRadius = currZone.minDensityPSD;
				float currentMaxRadius = currZone.maxDensityPSD;

				Vector2 candidate = spawnCentre + dir * Random.Range(currentMinRadius, currentMaxRadius);

				Zone candidateZone = GetZone(candidate);
				float candidateMinRadius = candidateZone.minDensityPSD;
				float candidateMaxRadius = candidateZone.maxDensityPSD;


				if (IsValid(candidate, sampleRegionSize, cellSize, candidateMinRadius, candidateMaxRadius, points, grid)) 
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

		Zone GetZone(Vector2 location)
        {
			float elevationYlocal = GetLocalY(location.x, location.y, planeObj);
			float candidateElevation = Mathf.InverseLerp(0f, 50f, elevationYlocal);

			foreach (Zone zone in zones)
			{
				// for now selecting zone based on elevation
				if (zone.elevationMin <= candidateElevation && candidateElevation <= zone.elevationMax)
				{
					return zone;
				}
			}
			return null;
		}

		(float, float) GetZoneRadius(Vector2 spawnCentre)
		{
			float elevationYlocal = GetLocalY(spawnCentre.x, spawnCentre.y, planeObj);
			float candidateElevation = Mathf.InverseLerp(0f, 50f, elevationYlocal);

			foreach (Zone zone in zones)
			{
				// for now selecting zone based on elevation
				if (zone.elevationMin <= candidateElevation && candidateElevation <= zone.elevationMax)
				{
					(float, float) candidateRadius = (zone.minDensityPSD, zone.maxDensityPSD);
					return candidateRadius;
				}
			}

			return (-1f, -1f);
		}
	}


	// checks the surrounding cells around a candidate
	// to make sure there aren't any too close to it which would invalidate it
	static bool IsValid(Vector2 candidate, Vector2 sampleRegionSize, float cellSize, float minRadius, float maxRadius, List<Vector2> points, int[,] grid) {
		
		// check if candidate is within the sample region on the map
		if (candidate.x >=0 && candidate.x < sampleRegionSize.x && candidate.y >= 0 && candidate.y < sampleRegionSize.y) 
		{	
			// find which cell the point lies in so we can search surrounding cells		
			int cellX = (int)(candidate.x/cellSize);
			int cellY = (int)(candidate.y/cellSize);

			// this gets the bounds of the area we are searching.
			// the min/max is to make sure points aren't off map
			// Seb has it simply as -2 or +2 because his range is fixed between radius and radius*2,
			// We would want that to be based on the minRadius and maxRadius, so need to calculate number of squares for maxRadius (DONE!)
			// min/max so that we don't try to get cells off the edge of the map
			int searchSize = CalculateSearchArea(maxRadius);

			int searchStartX = Mathf.Max(0, cellX-searchSize);
			int searchEndX = Mathf.Min(cellX+searchSize, grid.GetLength(0)-1);
			int searchStartY = Mathf.Max(0, cellY-searchSize);
			int searchEndY = Mathf.Min(cellY+searchSize, grid.GetLength(1)-1);

			for (int x = searchStartX; x <= searchEndX; x++) {
				for (int y = searchStartY; y <= searchEndY; y++) {

					// if pointIndex = -1 there is NO point in that spot
					int pointIndex = grid[x,y]-1;

					// so if there is a point in the cell,
					// check its distance and return IsValid=false if point is less than minRadius 
					if (pointIndex != -1) {
						float sqrDst = (candidate - points[pointIndex]).sqrMagnitude; //sqrMagnitude is less expensive than Magnitude so doing this with squared values
						if (sqrDst < minRadius*minRadius) {
							return false;
						}
					}
				}
			}
			return true;
		}
		return false; //return false if point not on map

		int CalculateSearchArea(float maxRadius)
        {
			return Mathf.CeilToInt(maxRadius / cellSize);
        }
	}



	

	

	public static float GetLocalY(float x, float y, GameObject obj)
    {
		RaycastHit hit;
		Ray ray = new Ray(new Vector3(x, 300, y), Vector3.down);
		if (obj.GetComponent<Collider>().Raycast(ray, out hit, 500))
		{
			return obj.transform.InverseTransformPoint(hit.point).y;
		}
		else return -999f;
    }





}
