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



        float[,] noiseMap;
        float elevationLimit;

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

                AddFourNeighbors(coords.x, coords.y, queue, null, null, null);
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
                        AddFourNeighbors(coords.x, coords.y, queue, null, null, null);
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
                AddFourNeighbors(current.x, current.y, queue, null, null, null);

                //Thinking about running a Func<> through the params to determine what to do with the found tiles


            }
        }

        public Dictionary<string, List<(int x, int y)>> AStar((int x, int y) start, (int x, int y) end, float[,] nm, float el)
        {
            var pathData = new Dictionary<string, List<(int x, int y)>>();

            // (List<(int x, int y)> ,List<(int x, int y)>)
            noiseMap = nm;
            elevationLimit = el;

            List<(int x, int y)> path = new List<(int x, int y)>();
            List<(int x, int y)> frontier = new List<(int x, int y)>();
            List<(int x, int y)> badPaths = new List<(int x, int y)>();


            //Debug.Log("======== Astar =======");
            //Debug.Log($"Running AStar: ({start.Item1},{start.Item2}) ---> ({end.Item1},{end.Item2})");

            path.Add(start);
            AddEightNeighbors(start.Item1, start.Item2, null, frontier, path, badPaths); //Override AddFourNeighbors to accept a list object
            int lowestCost = int.MaxValue; //Set to something huge, this also might be a float, idk
            var lowestCandidate = (int.MaxValue, int.MaxValue);  //For storing the tuple that has lowestCost
            bool retracing = false;

            //for (int x = 0; x < 10000; x++) //this is for quick debug to keep me getting stuck in the while loop 
            while (!path.Contains(end))
            { 
                for (int i = 0; i < frontier.Count; i++)
                {   
                    var candidate = frontier[i];
                    var candidate_x = candidate.Item1;
                    var candidate_y = candidate.Item2;

                    // if not traversible skip this node
                    if (noiseMap[candidate_x, candidate_y] > elevationLimit) continue;
                    // if in badPath also skip node
                    if (badPaths.Contains((candidate_x, candidate_y))) continue;
                    // ignore path unless retracing steps...idk about this one lol
                    if (!retracing && path.Contains(candidate)) continue;

                    if (candidate == end)
                    {
                        path.Add(candidate);
                        pathData.Add("path", path);
                        pathData.Add("badPaths", badPaths);
                        return pathData;
                    }

                    if (frontier.Count==1 && candidate==start) // idk if this works... should try to retrace its steps back to the start if there's no path?
                    {
                        path = null;
                        pathData.Add("path", path);
                        pathData.Add("badPaths", badPaths);
                        return pathData;
                    }

                    var gCost = Helpers.ManhattanDistance(candidate_x, start.Item1, candidate_y, start.Item2); //dist to start
                    var hCost = Helpers.ManhattanDistance(candidate_x, end.Item1, candidate_y, end.Item2); //dist to end
                    var fCost = gCost + hCost; // start dist + end dist

                    //Debug.Log(candidate+$"  gCost {gCost},  hCost {hCost},  fCost {fCost},");
                    
                    if (hCost < lowestCost) 
                    {
                        lowestCost = hCost;
                        lowestCandidate = candidate;
                    }

                    // Why does fCost not work? I thought that was how you were supposed to do this?
                    /*
                    if (fCost < lowestCost) 
                    {
                        lowestCost = fCost;
                        lowestCandidate = candidate;
                    }
                    */
                }

                // if no good nodes found
                if (lowestCandidate == (int.MaxValue, int.MaxValue))
                {
                    //Debug.Log($"No good node at {path[path.Count-1]}, added to badPaths");                    
                    retracing = true;
                    badPaths.Add((path[path.Count-1]));
                    AddEightNeighbors(lowestCandidate.Item1, lowestCandidate.Item2, null, frontier, path, badPaths);
                }
                else
                {   //Debug.Log($"lowestCandidate is {lowestCandidate}, finding new neighbors...");
                    retracing = false;
                    lowestCost = int.MaxValue; //Reset for next loop
                    path.Add(lowestCandidate);
                    frontier.Clear();
                    AddEightNeighbors(lowestCandidate.Item1, lowestCandidate.Item2, null, frontier, path, badPaths);
                    lowestCandidate = (int.MaxValue, int.MaxValue); //reset lowest candidate
                }
                //Debug.Log($"======================================================================");

            }

            Debug.Log("if you're seeing this, there was a fucky wucky with Pathfinding.Astar() ... Broke out of loop without finding path OR returning null...");
            return pathData; // this will never be called...hopefully
        }

        private void AddFourNeighbors(int x, int y, Queue<(int x, int y)> q, List<(int x, int y)> frontier, List<(int x, int y)> path, List<(int x, int y)> badPaths)
        {
            AddNeighborToQueue(x - 1, y, q, frontier, path, badPaths);
            AddNeighborToQueue(x + 1, y, q, frontier, path, badPaths);
            AddNeighborToQueue(x, y - 1, q, frontier, path, badPaths);
            AddNeighborToQueue(x, y + 1, q, frontier, path, badPaths);
        }

        private void AddEightNeighbors(int x, int y, Queue<(int x, int y)> q, List<(int x, int y)> frontier, List<(int x, int y)> path, List<(int x, int y)> badPaths)
        {
            AddFourNeighbors(x, y, q, frontier, path, badPaths);
            AddNeighborToQueue(x - 1, y - 1, q, frontier, path, badPaths);
            AddNeighborToQueue(x + 1, y - 1, q, frontier, path, badPaths);
            AddNeighborToQueue(x - 1, y + 1, q, frontier, path, badPaths);
            AddNeighborToQueue(x + 1, y + 1, q, frontier, path, badPaths);
        }

        private void AddNeighborToQueue(int x, int y, Queue<(int x, int y)> q, List<(int x, int y)> frontier, List<(int x, int y)> path, List<(int x, int y)> badPaths)
        {
            if (Map.IsValidTilePosition(x, y) && !visited.Contains((x, y)))
            {
                if (q != null)
                {
                    q.Enqueue((x, y));
                }
            }


            if (Map.IsValidTilePosition(x, y)) 
            {
                frontier.Add((x, y));
            }
        }
    }
}