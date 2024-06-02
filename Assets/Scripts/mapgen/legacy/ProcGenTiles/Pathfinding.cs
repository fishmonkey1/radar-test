using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using Unity.VisualScripting;
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

        public List<List<Tile>> MarkLandmassRegions(float[,] noiseMap, float elevationLimit)
        {
            List<List<Tile>> allRegions = new List<List<Tile>>();
            int regionLabel = 1;
            for (int width = 0; width < Map.Width; width++)
            {
                for (int height = 0; height < Map.Height; height++)
                { //Loop through all tiles on the map
                  //Check if the tile has a land code and assign it based on the elevationLimit
                  //If the tile is over the elevationLimit, do a DFS or BFS on all neighbors above the limit and label
                    Tile t = Map.GetTile(width, height);
                    if (!t.ValuesHere.ContainsKey("Elevation"))
                    { //Raise an error ifg you run the landmass marking without elevation data set
                        throw new System.Exception("Cannot floodfill landmasses without elevation data!");
                    }
                    else
                    { //There's elevation data here, so we check if it's under the elevation limit
                        if (t.ValuesHere["Elevation"] < elevationLimit )
                        { //This is not a raised landmass and just needs to have its Land value added as zero
                            if (!t.ValuesHere.ContainsKey("Region"))
                            {
                                t.ValuesHere.Add("Land", 0); //This takes a float, but we'll use 0 and 1 anyways :P
                                t.ValuesHere.Add("Region", 0); //All flat ground will be region zero, regardless of connection
                            }
                            
                        }
                        else
                        { //The tile is above the elevation limit and needs to be checked
                            if (!t.ValuesHere.ContainsKey("Region"))
                            {//If this hasn't had a region assigned then we need to floodfill this area
                                t.ValuesHere.Add("Land", 1); //We'll still mark the land here anyways, cause fuggit
                                //Do something with the returned region later, i guess?
                                List<Tile> region = FloodfillRegion(regionLabel, width, height, elevationLimit);
   
                                allRegions.Add(region); //Just stuff it in here for now :c
                                regionLabel++;
                            }
                        }
                    }
                }
            }
            return allRegions;
        }

        private List<Tile> FloodfillRegion(int regionNumber, int x, int y, float elevationLimit)
        { //Provide an override if you want to pass just the ints
            return FloodfillRegion(regionNumber, (x, y), elevationLimit);
        }

        private List<Tile> FloodfillRegion(int regionNumber, (int x, int y) coords, float elevationLimit)
        {
            
            Tile startTile = Map.GetTile(coords);
            startTile.ValuesHere.Add("Region", regionNumber); //Mark the first tile with the region number
            List<Tile> regionTiles = new List<Tile>(); //For holding all the tiles that are found
            regionTiles.Add(startTile);
            Queue<Tile> frontier = new Queue<Tile>(); //All eligible neighbors we've found
            frontier.Enqueue(startTile);

            while (frontier.Count != 0)
            { //time to start finding neighbors
                Tile t = frontier.Dequeue();
                List<Tile> neighbors = GetFourNeighborsList(t.x, t.y, TileOverElevation, checkFloat: elevationLimit);
                if (neighbors.Count == 0)
                {
                    Debug.Log($"No valid neighbors found during the floodfill at {t.x},{t.y}");
                    continue; //Keep going, we found nothing
                }
                
                foreach (Tile neighbor in neighbors)
                { //Time to check if the neighbor has been checked already
                    if (!regionTiles.Contains(neighbor))
                    { //This is land that hasn't been assigned a region code, so lets fix that
                        if (neighbor.ValuesHere.ContainsKey("Region"))
                            continue; //Don't try to mark regions that have already been marked
                        neighbor.ValuesHere.Add("Region", regionNumber);
                        regionTiles.Add(neighbor);
                        List<Tile> neighborNeighbors = GetFourNeighborsList(neighbor.x, neighbor.y, TileOverElevation, checkFloat: elevationLimit);
                        foreach (Tile tile in neighborNeighbors)
                        {
                            if (!regionTiles.Contains(tile))
                            { //Make sure we aren't adding tiles we've already marked
                                frontier.Enqueue(tile);
                            }
                        }
                    }
                }
            }
            //if (regionTiles.Count > 1) Debug.Log("Created region with tile size of: " + regionTiles.Count);

            return regionTiles;
        }


        public Dictionary<string, List<(int x, int y)>> AStar((int x, int y) start, (int x, int y) end, float[,] nm, float el)
        {
            var pathData = new Dictionary<string, List<(int x, int y)>>();

            // (List<(int x, int y)> ,List<(int x, int y)>)
            float[,] noiseMap = nm;
            float elevationLimit = el;

            List<(int x, int y)> _path = new List<(int x, int y)>();
            List<(int x, int y)> _frontier = new List<(int x, int y)>();
            List<(int x, int y)> _badPaths = new List<(int x, int y)>();


            //Debug.Log("======== Astar =======");
            Debug.Log($"Running AStar: ({start.x},{start.y}) ---> ({end.x},{end.y})");

            _path.Add(start);
            AddEightNeighbors(start.Item1, start.Item2, null, _frontier, _path, _badPaths); //Override AddFourNeighbors to accept a list object
            int lowestCost = int.MaxValue; //Set to something huge, this also might be a float, idk
            var lowestCandidate = (int.MaxValue, int.MaxValue);  //For storing the tuple that has lowestCost
            bool retracing = false;

            for (int x = 0; x < 20000; x++) //this is for quick debug to keep me getting stuck in the while loop 
            //while (!_path.Contains(end))
            {
                for (int i = 0; i < _frontier.Count; i++)
                {
                    var candidate = _frontier[i];
                    var candidate_x = candidate.Item1;
                    var candidate_y = candidate.Item2;

                    // if in badPath skip node
                    if (_badPaths.Contains((candidate_x, candidate_y))) continue;

                    // if not traversible skip this node
                    if (noiseMap[candidate_x, candidate_y] > elevationLimit) 
                    {
                        continue; 
                    }
                   
                    // ignore path unless retracing steps...idk about this one lol
                    if (!retracing && _path.Contains(candidate)) continue;

                    if (candidate == end)
                    {
                        Debug.Log("FoundEnd at cycle: "+x);
                        _path.Add(candidate);
                        pathData.Add("path", _path);
                        pathData.Add("badPaths", _badPaths);
                        return pathData;
                    }

                    if (_frontier.Count == 1 && candidate == start) // idk if this works... should try to retrace its steps back to the start if there's no path?
                    {
                        _path = null;
                        pathData.Add("path", _path);
                        pathData.Add("badPaths", _badPaths);
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
                    Tile t = Map.GetTile(_path[_path.Count - 1]);

                    // need to only add neighbor to _badPaths if it's over elevation
                    // we;re adding to badpaths EVERy time we go backwards.
                    // THIS IS WHERE THE BUG IS
                    _badPaths.Add((_path[_path.Count - 1]));

                    AddEightNeighbors(lowestCandidate.Item1, lowestCandidate.Item2, null, _frontier, _path, _badPaths);
                }
                else
                {   //Debug.Log($"lowestCandidate is {lowestCandidate}, finding new neighbors...");
                    retracing = false;
                    lowestCost = int.MaxValue; //Reset for next loop
                    _path.Add(lowestCandidate);
                    _frontier.Clear();
                    AddEightNeighbors(lowestCandidate.Item1, lowestCandidate.Item2, null, _frontier, _path, _badPaths);
                    lowestCandidate = (int.MaxValue, int.MaxValue); //reset lowest candidate
                }
                //Debug.Log($"======================================================================");

            }

            Debug.Log("  This path ^^^ did not reach the end of it's path. This Start --> End combination will cause a crash in Astar's while loop.");
            Debug.Log("===============================================================================================================================");

            pathData.Add("path", _path);
            pathData.Add("badPaths", _badPaths);
            return pathData; // this will never be called...hopefully
        }


        // TODO: Have these return the neighbors instead of setting them directly.

        /// <summary>
        /// Returns a list of four neighboring tiles using the checkFunction passed
        /// </summary>
        /// <param name="coords"></param>
        /// <param name="checkFunction"></param>
        /// <param name="optionalAddList"></param>
        /// <returns>List<(int x, int y)></returns>
        private List<Tile> GetFourNeighborsList((int x, int y) coords, Func<Tile, float, bool> checkFunction, List<Tile> optionalAddList = null, float checkFloat = 0)
        {
            bool debug = false;
            List<Tile> foundNeighbors = null;
            if (optionalAddList == null)
                foundNeighbors = new List<Tile>();
            else
                foundNeighbors = optionalAddList;
            Tile start = Map.GetTile(coords);

            Tile west = Map.GetTile(coords.x - 1, coords.y);
            Tile east = Map.GetTile(coords.x + 1, coords.y);
            Tile north = Map.GetTile(coords.x, coords.y + 1);
            Tile south = Map.GetTile(coords.x, coords.y - 1);

            //Run the found tile through the function that checks if we should add it
            if (east != null)
            {
                if (checkFunction(east, checkFloat))
                {
                    foundNeighbors.Add(east);
                }
                else
                    debug = true;

            }
            if (west != null)
            {
                if (checkFunction(west, checkFloat))
                {
                    foundNeighbors.Add(west);
                }
                else
                    debug = true;
            }

            if (north != null)
            {
                if (checkFunction(north, checkFloat))
                {
                    foundNeighbors.Add(north);
                }
                else
                    debug = true;
            }

            if (south != null)
            {
                if (checkFunction(south, checkFloat))
                {
                    foundNeighbors.Add(south);
                }
                else
                    debug = true;
            } 
            
            if (debug)
            {
                Debug.Log($"Neighbors check. Start is {start.x},{start.y} with elevation {start.ValuesHere["Elevation"]}");
                foreach (var neighbor in foundNeighbors)
                    Debug.Log($"Found neighbor at {neighbor.x},{neighbor.y} with elevation {neighbor.ValuesHere["Elevation"]}");
            }

            return foundNeighbors;
        }

        /// <summary>
        /// Optional override in case you don't want to pack your own tuple :3
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="checkFunction"></param>
        /// <param name="optionalAddList"></param>
        /// <returns></returns>
        private List<Tile> GetFourNeighborsList(int x, int y, Func<Tile, float, bool> checkFunction, List<Tile> optionalAddList = null, float checkFloat = 0)
        {
            return GetFourNeighborsList((x, y), checkFunction, optionalAddList, checkFloat);
        }

        private bool TileOverElevation(Tile t, float elevationLimit)
        {
            if (t.ValuesHere["Elevation"] > elevationLimit)
            {
                return true;
            } else
            {
                return false;
            }
                
            
        }

        private void AddFourNeighbors(int x, int y, Queue<(int x, int y)> q, List<(int x, int y)> frontier, List<(int x, int y)> path, List<(int x, int y)> badPaths)
        {
            List<(int x, int y)> neighbors = new List<(int x, int y)>();

            AddNeighborToQueue(x - 1, y, q, frontier, path, badPaths);
            AddNeighborToQueue(x + 1, y, q, frontier, path, badPaths);
            AddNeighborToQueue(x, y - 1, q, frontier, path, badPaths);
            AddNeighborToQueue(x, y + 1, q, frontier, path, badPaths);
        }

        private void AddEightNeighbors(int x, int y, Queue<(int x, int y)> q, List<(int x, int y)> frontier, List<(int x, int y)> path, List<(int x, int y)> badPaths)
        {
            List<(int x, int y)> neighbors = new List<(int x, int y)>();

            AddFourNeighbors(x, y, q, frontier, path, badPaths);
            AddNeighborToQueue(x - 1, y - 1, q, frontier, path, badPaths);
            AddNeighborToQueue(x + 1, y - 1, q, frontier, path, badPaths);
            AddNeighborToQueue(x - 1, y + 1, q, frontier, path, badPaths);
            AddNeighborToQueue(x + 1, y + 1, q, frontier, path, badPaths);
        }

        private void AddNeighborToQueue(int x, int y, Queue<(int x, int y)> q, List<(int x, int y)> frontier, List<(int x, int y)> path, List<(int x, int y)> badPaths)
        {
/*            if (Map.IsValidTilePosition(x, y) && !visited.Contains((x, y))) //
            {
            
                    q.Enqueue((x, y));
                
            }*/ //won't be using this much longer

            
            if (Map.IsValidTilePosition(x, y))
            {
                frontier.Add((x, y));
            }
        }
    }
}