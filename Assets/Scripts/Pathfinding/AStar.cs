using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Homemade AStar implementation for use on <see cref="Graph"/> and <see cref="Node"/> objects.
/// </summary>
public static class AStar
{
    static List<Node> Visited = new();

    /// <summary>
    /// Find a node in the graph from a starting point.
    /// </summary>
    /// <param name="start">The node to start the search from.</param>
    /// <param name="end">The node to find at the end of the path.</param>
    /// <returns>A list containing the path found from the start node to the end node.</returns>
    public static List<Node> GetPath(Node start, Node end)
    {
        List<Node> Path = new(); //Don't add the start node here since it'll be done on the first while loop
        Queue<Node> Frontier = new();
        Visited.Clear(); //Refresh the visited list for our use
        Frontier.Enqueue(start);
        Node check = null;
        while (check != end)
        { //Start searching for nodes that are closest to the end node and build up a path from them
            check = Frontier.Dequeue(); //Get the next node we should search for
            if (!Visited.Contains(check))
                Visited.Add(check); //No need to duplicate values in the list from backtracking
            Node closestConnection = null;
            float bestDistance = float.MaxValue;
            foreach (Node node in check.Connections)
            {
                if (Visited.Contains(node))
                    continue; //Skip nodes we've already searched
                float distance = GetEuclideanDistance(node, end);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    closestConnection = node;
                }
            }
            //This is likely our best path, so continue following it
            if (closestConnection != null)
            {
                Frontier.Enqueue(closestConnection);
                Path.Add(check); //Place our node along the path
            }
            else
            { //We checked all of the nodes and had already visited them all, so we must be in a dead end
                //We need to backtrack and remove the last node from the path, starting there
                Node backtrack = Path[Path.Count - 1];
                Path.Remove(backtrack); //Get rid of the last entry and try again there
                Frontier.Enqueue(backtrack); //I don't think anything else should be here in the frontier, so this should be next? Need to test
            }
        }

        return Path; //And send back our located path
    }

    /// <summary>
    /// Helper function for checking shortest distances between nodes.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public static float GetEuclideanDistance(Node start, Node end)
    {
        return Vector3.Distance(start.transform.position, end.transform.position);
    }

    /// <summary>
    /// Count up the length of a path that was found.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static float GetPathDistance(List<Node> path)
    {
        float totalDistance = 0;
        Node last = null;
        Node current = null;
        foreach (Node node in path)
        {
            if (last == null)
            {
                last = node;
                continue; //Just skip ahead to the next node so we can calculate the distances
            }
            last = current;
            current = node;
            totalDistance += GetEuclideanDistance(last, current);
        }
        return totalDistance;
    }

    /// <summary>
    /// Helper function for visualizing paths along the graph.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string PrintPath(List<Node> path)
    {
        string pathString = "Path: ";
        foreach (Node node in path)
        {
            pathString += node.gameObject.name + " -> ";
        }
        pathString += "End of path";
        return pathString;
    }
}
