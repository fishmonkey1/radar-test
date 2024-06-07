using System.Collections.Generic;

namespace ProcGenTiles
{
    public class Tile
    {
        public int x, y;
        public Tile pathfindParent;
        public int gCost;
        public int hCost;
        public int fCost;
        public Dictionary<string, float> ValuesHere = new Dictionary<string, float>(); //Store generated values in here

        public Region Region; // hmmmmm so we're going to want to know if/what region a tile is in? so we can set this during the floodfill?
        public bool isBelowElevationLimit;

        public bool is69;
        public bool is420;
        public bool is42069;


        public Tile(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }
}