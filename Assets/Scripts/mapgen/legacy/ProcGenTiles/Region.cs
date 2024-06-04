using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProcGenTiles
{
	public class Region
	{
		public Tile[,] Tiles { get; set; }
		public Dictionary<string, List<List<Tile>>> Neighbors { get; set; }   // Store the neighboring regions here, for roadgen
																			  //	
																			  // "n" : [ [hulllpoints] , [hullpoints] ]
																			  // "e" : [ [hulllpoints] , [hullpoints] ]
																			  // "s" : [ [hulllpoints] , [hullpoints] ]
																			  // "w" : [ [hulllpoints] , [hullpoints] ]



	}
}