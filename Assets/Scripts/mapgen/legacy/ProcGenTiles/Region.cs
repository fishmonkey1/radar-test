using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProcGenTiles
{
	public class Region
	{
		public Tile[] Tiles { get; set; }
		public Tile[] HullPoints { get; set; }
		public (Tile,Tile)[] HullLines { get; set; } 

		public Color Color { get; set; }
		public string colorName { get; set; }


		/// <summary>
		/// "n" / "e" / "s" / "w"   :   [region1, region2]   
		/// </summary>	
		public Dictionary<string, Region[]> RegionNeighbors { get; set; }

		public Dictionary<string, List<(float, float)[]>> NeighborMidpoints { get; set; }

		/// <summary>
		/// <code>
		/// region.Bounds[0] = (w, n); --> top-left (x,y) 
		/// region.Bounds[1] = (e, s); --> bottom-right (x,y) 
		/// </code> 
		/// </summary>																
		public (int x,int y)[] Bounds { get; set; }



	}
}