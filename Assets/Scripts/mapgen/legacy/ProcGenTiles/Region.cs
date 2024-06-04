using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProcGenTiles
{
	public class Region
	{
		public Tile[] Tiles { get; set; }
		public Tile[] HullPoints { get; set; }

		public Dictionary<string, Region[]> RegionNeighbors { get; set; }   // "n" : [region1, region2]
																			// "e" : [region1, region2]
																			// "s" : [ ]
																			// "w" : [region1]



	}
}