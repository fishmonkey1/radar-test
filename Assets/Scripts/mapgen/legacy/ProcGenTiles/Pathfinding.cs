using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ProcGenTiles
{
    public class Pathfinding
    {
        HashSet<(int x, int y)> visited = new HashSet<(int x, int y)>();
        Queue<(int x, int y)> queue = new Queue<(int x, int y)>();
        Map Map;
        public Dictionary<int, int> regionSizes = new Dictionary<int, int>(); //Holds the region index and the number of tiles marked with it, for size checking

        public Pathfinding(Map map)
        {
            Map = map;
        }

        public void LandWaterFloodfill(int x, int y, Biomes biomes)
        {
            LandWaterFloodfill((x, y), biomes);
        }

        public void LandWaterFloodfill((int x, int y) start, Biomes biomes)
        {
            queue.Clear();
            queue.Enqueue(start);
            visited.Clear();
            visited.Add(start);

            float waterElevation = biomes.GetWaterLayer().value; //Find the layer marked as water height and use it in the floodfill

            while (queue.Count > 0)
            {
                (int x, int y) coords = queue.Dequeue();
                Tile tile = Map.GetTile(coords); //Can always assume value is not null due to AddNeighborsToQueue

                //Check the elevation layer, if it doesn't exist exit with an error
                if (!tile.ValuesHere.ContainsKey("Elevation"))
                {
                    //throw new InvalidOperationException("Cannot floodfill without elevation data");
                }

                if (tile.ValuesHere["Elevation"] >= waterElevation)
                    tile.ValuesHere.Add("Land", 1); //Heck this only takes floats so we'll use positive 1 for true and 0 for false
                else
                    tile.ValuesHere.Add("Land", 0);

                AddFourNeighbors(coords.x, coords.y, queue);
            }
        }

        public void MarkAllRegions()
        {
            //Get a list of all tiles
            List<(int x, int y)> values = new List<(int x, int y)>();
            for (int x = 0; x < Map.Width; x++)
            {
                for (int y = 0; y < Map.Height; y++)
                {
                    values.Add((x, y));
                }
            }
            //Move first tile from list to frontier
            Queue<(int x, int y)> frontier = new Queue<(int x, int y)>();
            visited.Clear();
            int region = 0; //Track which region we're marking
                            //Add neighbors to frontier if Land values match

            while (values.Count > 0)
            { //Loop for all tiles
                frontier.Enqueue(values[0]);
                visited.Add(values[0]);
                Tile compare = Map.GetTile(values[0]); //For checking the Land value
                if (!regionSizes.ContainsKey(region))
                { //If there's no region entry for this, we should add it
                    regionSizes.Add(region, 0);
                }

                while (frontier.Count > 0)
                {
                    (int x, int y) coords = frontier.Dequeue();
                    Tile found = Map.GetTile(coords);
                    if (found.ValuesHere["Land"] == compare.ValuesHere["Land"])
                    { //The neighbor matches the start value so assign them the same region
                        found.ValuesHere.TryAdd("Region", region);
                        regionSizes[region]++; //Increment the number of tiles in the region
                        values.Remove(coords); //Delete from values if region is marked
                        AddFourNeighbors(coords.x, coords.y, frontier);
                    }
                }
                region++; //On to the next one if the frontier ran out
                visited.Clear();
            }
            //Mark until frontier is empty, removing values from the list
            //increment region and pop first item from list until list is empty
        }

        public void BFS((int x, int y) start)
        {

            visited.Add(start);
            queue.Enqueue(start);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                // Add neighboring tiles to the queue if not visited for 4 dir pathfinding: diamonds
                AddFourNeighbors(current.x, current.y, queue);

                //Thinking about running a Func<> through the params to determine what to do with the found tiles


            }
        }

        public List<(int x, int y)> AStar((int x, int y) start, (int x, int y) end)
        {

            List<(int x, int y)> path = new List<(int x, int y)>();
            path.Add(start);
            List<(int x, int y)> frontier = new List<(int x, int y)>();
            //AddFourNeighbors(start, frontier); //Override AddFourNeighbors to accept a list object
            int lowestCost = 9999999; //Set to something huge, this also might be a float, idk
            (int x, int y) lowestCandidate = (0,0);  //For storing the tuple that has lowestCost


            while (!path.Contains(end))
            {
                for (int i = 0; i < frontier.Count; i++)
                {
                    //Maybe remove any tiles that are already in the path here
                    var candidate = frontier[i];
                    var manhattanCost = Helpers.ManhattanDistance(start.Item1, end.Item1, start.Item2, end.Item2);
                    if (manhattanCost < lowestCost)
                    {
                        lowestCost = manhattanCost;
                        lowestCandidate = candidate;
                    }
                }
                //Do this code after the for loop has run so we know it checked all the neighbors
                lowestCost = 9999999; //Reset for next loop
                path.Add(lowestCandidate);




                frontier.Clear();
                AddFourNeighbors(lowestCandidate.Item1, lowestCandidate.Item2, null, frontier);
            }

            return path;




        }

        private void AddFourNeighbors(int x, int y, Queue<(int x, int y)> q = null, List<(int x, int y)> l = null)
        {
            AddNeighborToQueue(x - 1, y, q);
            AddNeighborToQueue(x + 1, y, q);
            AddNeighborToQueue(x, y - 1, q);
            AddNeighborToQueue(x, y + 1, q);
        }

        private void AddEightNeighbors(int x, int y, Queue<(int x, int y)> q)
        { //Stubbed just in case
            AddFourNeighbors(x, y, q);
            AddNeighborToQueue(x - 1, y - 1, q);
            AddNeighborToQueue(x + 1, y - 1, q);
            AddNeighborToQueue(x - 1, y + 1, q);
            AddNeighborToQueue(x + 1, y + 1, q);
        }

        private void AddNeighborToQueue(int x, int y, Queue<(int x, int y)> q)
        {
            Debug.Log("Map is: " + Map);
            if (Map.IsValidTilePosition(x, y) && !visited.Contains((x, y)))
                q.Enqueue((x, y));
            visited.Add((x, y));
        }
    }
}