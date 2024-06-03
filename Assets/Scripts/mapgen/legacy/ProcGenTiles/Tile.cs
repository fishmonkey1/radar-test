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

        public Tile(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }
}