using Unity.VisualScripting;

namespace ProcGenTiles
{
	public class Map
	{
		public Tile[,] Tiles { get; set; }
		public Region[] Regions { get; set; }
		public int Width, Height;

		public Map(int width, int height)
		{
			Width = width;
			Height = height;
			Tiles = new Tile[width, height];
			for (int y = 0; y < height; y++){
				for (int x = 0; x < width; x++){
					Tiles[x,y] = new Tile(x, y);
				}
			}
		}
		
		public bool IsValidTilePosition(int x, int y)
		{
			return x >= 0 && x < Tiles.GetLength(0) && y >= 0 && y < Tiles.GetLength(1);
		}
		
		public Tile GetTile((int x, int y) coords)
		{
			if (IsValidTilePosition(coords.x, coords.y))
			{
				return Tiles[coords.x, coords.y];
			}
			return null; //If it isn't a valid tile return null
		}
		
		public Tile GetTile(int x, int y)
		{
			return GetTile((x, y));
		}

		public Region GetRegion((int x, int y) coords) 
		{
			if (IsValidTilePosition(coords.x, coords.y))
			{	foreach (Region reg in Regions)
                {	
					foreach (Tile t in reg.Tiles)
                    {
						if (coords == (t.x, t.y))
                        {
							return reg;
                        }
                    }
                }
				return null; // if the coords are not in a region return null
			}	
			return null; //If it isn't a valid tile return null
		}

		public Region GetRegion(int x, int y)
		{
			return GetRegion((x, y));
		}


		public float[,] FetchFloatValues(string layer)
		{
			float[,] array = new float[Width, Height];
			for (int y = 0; y < Height; y++)
			{
				for (int x = 0; x < Width; x++)
				{
					Tile tile = GetTile(x, y);
					if (!tile.ValuesHere.ContainsKey(layer))
					{
						throw new System.ArgumentException("No such layer is present on the tile to fetch!"); //This should crash the function and alert the editor.
					}
					else
					{
						array[x, y] = tile.ValuesHere[layer];
					}
				}
			}
			return array;
		}
		public float[,] FetchFloatValues_ReversedYXarray(string layer)
		{
			float[,] array = new float[Width, Height];
			for (int y = 0; y < Height; y++)
			{
				for (int x = 0; x < Width; x++)
				{
					Tile tile = GetTile(x, y);
					if (!tile.ValuesHere.ContainsKey(layer))
					{
						throw new System.ArgumentException("No such layer is present on the tile to fetch!"); //This should crash the function and alert the editor.
					}
					else
					{
						array[y, x] = tile.ValuesHere[layer];
					}
				}
			}
			return array;
		}

		public float[,] FetchFloatValuesSlice(string layer, int minY, int maxY, int minX, int maxX)
		{
			float[,] array = new float[maxX - minX, maxY - minY];

			for (int y = 0; y < maxY - minY; y++)
			{
				for (int x = 0; x < maxX - minX; x++)
				{
					Tile tile = GetTile(x + minX, y + minY);
                    if (!tile.ValuesHere.ContainsKey(layer))
                    {
                        throw new System.ArgumentException("No such layer is present on the tile to fetch!"); //This should crash the function and alert the editor.
                    }
                    else
                    {
                        array[x, y] = tile.ValuesHere[layer];
                    }
                }
			}

			return array;
		}
	}
}