using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProcGenTiles
{
	public class Region
	{
		public Tile[,] Tiles { get; set; }
		public Tile[] HullPoints { get; set; }
		public Dictionary<string, List<List<Tile>>> RegionNeighbors { get; set; }   // Store the neighboring regions here for now, for roadgen
																					// It's gonna be the hullpoints instead of the whole Tile array eventually,
																					// but for now just store tiles, or some link idfk :3
																					// "n" : [ [hulllpoints] , [hullpoints] ]
																					// "e" : [ [hulllpoints] , [hullpoints] ]
																					// "s" : [ [hulllpoints] , [hullpoints] ]
																					// "w" : [ [hulllpoints] , [hullpoints] ]



	}
}