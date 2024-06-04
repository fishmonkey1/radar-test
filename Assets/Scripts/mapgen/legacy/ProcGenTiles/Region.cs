using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProcGenTiles
{
	public class Region
	{
		public Tile[] Tiles { get; set; }
		public Tile[] HullPoints { get; set; }


		/// <summary>
		/// "n" / "e" / "s" / "w"   :   [region1, region2]   
		/// </summary>	
		public Dictionary<string, Region[]> RegionNeighbors { get; set; }   // "n" : [region1, region2]
																			// "e" : [region1, region2]
																			// "s" : [ ]
																			// "w" : [region1]


		/// <summary>
		/// <code>
		/// region.Bounds[0] = (w, n); --> top-left (x,y) 
		/// region.Bounds[1] = (e, s); --> bottom-right (x,y) 
		/// </code> 
		/// </summary>																
		public (int,int)[] Bounds { get; set; }



	}
}