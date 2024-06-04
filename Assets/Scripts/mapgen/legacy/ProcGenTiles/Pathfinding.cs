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
        //float elevationLimit;

        public Pathfinding(Map map)
        {
            Map = map;
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

        public List<Region> MarkLandmassRegions(float[,] noiseMap, float elevationLimit)
        {
            List<List<Tile>> allRegions = new List<List<Tile>>();
            List<Region> AllRegionObj = new List<Region>();
            int regionLabel = 1;
            for (int width = 0; width < Map.Width; width++)
            {
                for (int height = 0; height < Map.Height; height++)
                { //Loop through all tiles on the map
                  //Check if the tile has a land code and assign it based on the elevationLimit
                  //If the tile is over the elevationLimit, do a DFS or BFS on all neighbors above the limit and label
                    Tile t = Map.GetTile(width, height);
                    if (!t.ValuesHere.ContainsKey("Elevation"))
                    { //Raise an error if you run the landmass marking without elevation data set
                        throw new System.Exception("Cannot floodfill landmasses without elevation data!");
                    }
                    else
                    { //There's elevation data here, so we check if it's under the elevation limit
                        if (t.ValuesHere["Elevation"] < elevationLimit )
                        { //This is not a raised landmass and just needs to have its Land value added as zero
                            if (!t.ValuesHere.ContainsKey("Region"))
                            {
                                t.ValuesHere.Add("Land", 0); //This takes a float, but we'll use 0 and 1 anyways :P
                               
                                // old
                                t.ValuesHere.Add("Region", 0); //All flat ground will be region zero, regardless of connection
                            
                                // new
                                // ????
                            }
                            
                        }
                        else
                        { //The tile is above the elevation limit and needs to be checked
                            // old :c
                            if (!t.ValuesHere.ContainsKey("Region"))

                            // new :3
                            // if(t.Region == null)

                            {//If this hasn't had a region assigned then we need to floodfill this area
                                //Do something with the returned region later, i guess?
                                
                                // old YUCKY :c
                                List<Tile> region = FloodfillRegion(regionLabel, width, height, elevationLimit);
                                allRegions.Add(region); //Just stuff it in here for now :c
                                regionLabel++;

                                // new :3
                                Region regionObj = new Region();
                                regionObj.Tiles = FloodfillRegion(regionLabel, width, height, elevationLimit).ToArray();
                                AllRegionObj.Add(regionObj); //

                               


                            }
                        }
                    }
                }
            }
            return AllRegionObj;
        }

        private List<Tile> FloodfillRegion(int regionNumber, int x, int y, float elevationLimit)
        { //Provide an override if you want to pass just the ints
            return FloodfillRegion(regionNumber, (x, y), elevationLimit);
        }

        private List<Tile> FloodfillRegion(int regionNumber, (int x, int y) coords, float elevationLimit)
        {
            ;

            Tile startTile = Map.GetTile(coords);
            List<Tile> regionTiles = new List<Tile>(); //For holding all the tiles that are found, 
            Queue<Tile> frontier = new Queue<Tile>(); //All eligible neighbors we've found
            frontier.Enqueue(startTile);

            while (frontier.Count != 0)
            { //time to start finding neighbors
                Tile t = frontier.Dequeue();
                regionTiles.Add(t);
                t.ValuesHere.Add("Land", 1);
                t.ValuesHere.Add("Region", regionNumber);
                List<Tile> neighbors = GetNeighbors(t.x, t.y, TileOverElevation, eightNeighbors: false, checkFloat: elevationLimit);
                if (neighbors.Count == 0)
                {
                    //Debug.Log($"No valid neighbors found during the floodfill at {t.x},{t.y}");
                    continue; //Keep going, we found nothing
                }
                
                foreach(Tile neighbor in neighbors)
                {
                    if (frontier.Contains(neighbor) || regionTiles.Contains(neighbor))
                        continue; //Don't add tiles that have already been looked at
                    frontier.Enqueue(neighbor);
                }
            }

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
                    if (_badPaths.Contains(candidate)) continue;

                    // need to only add neighbor to _badPaths if it's over elevation
                    // we;re adding to badpaths EVERy time we go backwards.
                    // THIS IS WHERE THE BUG IS
                    Tile t = Map.GetTile(_path[_path.Count - 1]);
                    if (TileOverElevation(t, elevationLimit))
                    {
                        _badPaths.Add(candidate);
                        retracing = true;
                    }
                    

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
                if (lowestCandidate == (int.MaxValue, int.MaxValue) && retracing)
                {
                    //Debug.Log($"No good node at {path[path.Count-1]}, added to badPaths");                    

                    AddEightNeighbors(_path[_path.Count - 2].x, _path[_path.Count - 2].y, null, _frontier, _path, _badPaths);
                    _path.RemoveAt(_path.Count - 1);
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
        public List<Tile> GetNeighbors((int x, int y) coords, Func<Tile, float, bool> checkFunction, bool eightNeighbors, List<Tile> optionalAddList = null, float checkFloat = 0)
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

            CheckNeighbor(west);
            CheckNeighbor(east);
            CheckNeighbor(north);
            CheckNeighbor(south);

            if (eightNeighbors)
            {
                Tile ne = Map.GetTile(coords.x + 1, coords.y + 1);
                Tile se = Map.GetTile(coords.x + 1, coords.y - 1);
                Tile sw = Map.GetTile(coords.x - 1, coords.y - 1);
                Tile nw = Map.GetTile(coords.x - 1, coords.y + 1);

                CheckNeighbor(ne);
                CheckNeighbor(se);
                CheckNeighbor(sw);
                CheckNeighbor(nw);
            }

            void CheckNeighbor(Tile direction)
            {
                if (direction != null)
                {
                    if (checkFunction(direction, checkFloat))
                    {
                        foundNeighbors.Add(direction);
                    }
                    else
                        debug = true;
                }
            }
  
            if (debug)
            {
                Debug.Log($"Neighbors check. Start is {start.x},{start.y} with elevation {start.ValuesHere["Elevation"]}");
                foreach (var neighbor in foundNeighbors)
                    Debug.Log($"Found neighbor at {neighbor.x},{neighbor.y} with elevation {Math.Round( neighbor.ValuesHere["Elevation"] , 4)}");
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
        private List<Tile> GetNeighbors(int x, int y, Func<Tile, float, bool> checkFunction, bool eightNeighbors, List<Tile> optionalAddList = null, float checkFloat = 0 )
        {
            return GetNeighbors((x, y), checkFunction, eightNeighbors, optionalAddList, checkFloat);
        }

        /// <summary>
        /// Returns true if given tile elevation is GREATER THAN the elevationLimit.
        /// </summary>
        public bool TileOverElevation(Tile t, float elevationLimit)
        {
            if (t.ValuesHere["Elevation"] > elevationLimit)
            {
                return true;
            } else
            {
                return false;
            }
        }

        public bool TileUnderElevation(Tile t, float elevationLimit)
        {
            return !TileOverElevation(t, elevationLimit);
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


        public List<Tile> new_Astar(Tile start, Tile end, float elevationLimit)
        {   
            // adapted from Sebastian Lague's Pathfinding code.
            // Going to add comments to make it easier to read.
            // https://github.com/SebLague/Pathfinding/blob/master/Episode%2003%20-%20astar/Assets/Scripts/Pathfinding.cs

            List<Tile> finalPath = new List<Tile>();

            List<Tile> openSet = new List<Tile>();
            HashSet<Tile> closedSet = new HashSet<Tile>();
            openSet.Add(start);



            while (openSet.Count > 0)
            {
                Tile current = openSet[0];

                current.gCost = Helpers.ManhattanDistance(current.x, start.x, current.y, start.y); //dist to start
                current.hCost = Helpers.ManhattanDistance(current.x, end.x, current.y, end.y); //dist to end
                //current.fCost = current.gCost + current.hCost; // start dist + end dist
                current.fCost = current.hCost;

                for (int i = 1; i < openSet.Count; i++)
                {
                    var candidate = openSet[i];

                    candidate.gCost = Helpers.ManhattanDistance(candidate.x, start.x, candidate.y, start.y); //dist to start
                    candidate.hCost = Helpers.ManhattanDistance(candidate.x, end.x, candidate.y, end.y); //dist to end
                    //candidate.fCost = candidate.gCost + candidate.hCost; // start dist + end dist
                    candidate.fCost = candidate.hCost;

                    if (candidate.fCost < current.fCost || candidate.fCost == current.fCost)
                    {
                        if (candidate.hCost < current.hCost)
                            current = candidate;
                    }
                }

                openSet.Remove(current);
                closedSet.Add(current);

                if (current == end)
                {
                    Debug.Log("got to end");
                    RetracePath(start, end);
                    return finalPath;
                }

                List<Tile> neighbors = GetNeighbors(current.x, current.y, TileUnderElevation,eightNeighbors: true, checkFloat: elevationLimit);
                foreach (Tile neighbor in neighbors)
                {
                    if (closedSet.Contains(neighbor)) continue;

                    //set neighbors cost vals 
                    neighbor.gCost = Helpers.ManhattanDistance(neighbor.x, start.x, neighbor.y, start.y); //dist to start
                    neighbor.hCost = Helpers.ManhattanDistance(neighbor.x, end.x, neighbor.y, end.y); //dist to end
                    //neighbor.fCost = neighbor.gCost + neighbor.hCost; // start dist + end dist
                    neighbor.fCost = neighbor.hCost;

                    var dist_to_neighbor = Helpers.ManhattanDistance(current.x, neighbor.x, current.y, neighbor.y); //dist to start

                    int newCostToneighbor = current.gCost + dist_to_neighbor; //lmao                    
                    if (newCostToneighbor < neighbor.gCost || !openSet.Contains(neighbor))
                    {
                        neighbor.gCost = newCostToneighbor;
                        neighbor.hCost = Helpers.ManhattanDistance(neighbor.x, end.x, neighbor.y, end.y); //dist to end
                        neighbor.pathfindParent = current;
                        //Debug.Log($"added ({current.x},{current.y})   parent to   ({neighbor.x},{neighbor.y})");

                        if (!openSet.Contains(neighbor))
                            openSet.Add(neighbor);
                    }
                }
            }


            void RetracePath(Tile start, Tile end)
            {
                List<Tile> path = new List<Tile>();
                Tile currentNode = end;

                while (currentNode != start)
                {
                    path.Add(currentNode);

                    currentNode = currentNode.pathfindParent;
                }
                path.Reverse();

                finalPath = path;

            }

            return finalPath;

        }

    }





}