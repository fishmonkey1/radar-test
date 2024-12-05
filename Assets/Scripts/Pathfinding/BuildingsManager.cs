using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// Holds all of the buildings that were found on the graph, along with the building weights.
/// </summary>
public class BuildingsManager : MonoBehaviour
{
    public List<Node> BuildingNodes = new(); //If a node has a building, it ends up in here
    public Dictionary<Node, BuildingIntPair> WeightedNodeBuildings = new(); //Track the values of the nodes here

    /// <summary>
    /// Change the weight of a node, typically after an order includes the node.
    /// </summary>
    /// <param name="node"></param>
    /// <param name="change"></param>
    public void ChangeBuildingValue(Node node, int change)
    { //Doing this so I can hook debug stuff up later if we need
        BuildingIntPair pair = WeightedNodeBuildings[node];
        pair.Value += change;
        PrintWeightedNodes();
    }

    /// <summary>
    /// Get a list of all the buildings in the graph, sorted from the highest weighted to the lowest.
    /// </summary>
    /// <param name="numResults">How many buildings to fetch out of the list.</param>
    /// <param name="exclude">Any buildings types you don't want returned in the list.</param>
    /// <returns></returns>
    public List<Node> GetHighestValuedBuildings(int numResults, List<NodeBuilding> exclude = null)
    {
        exclude ??= new List<NodeBuilding>(); //Initialize the list if it doesnt get passed

        List<Node> topBuildings = WeightedNodeBuildings //Linq statement
        .Where(pair => !exclude.Contains(pair.Value.Building)) //Filter out any buildings that were excluded
        .OrderByDescending(pair => pair.Value.Value) //Sort from highest at the start to lowest
        .Take(numResults) //Only grab however many results they asked for
        .Select(pair => pair.Key) //Swap the value to Node instead of BuildingIntPair
        .ToList(); //And finally make it a list

        return topBuildings;
    }

    /// <summary>
    /// Helper for peaking into the created lists, typically for verifying JSON loading.
    /// </summary>
    public void PrintWeightedNodes()
    {
        string nodePairs = "Weighted nodes list: ";
        foreach (KeyValuePair<Node, BuildingIntPair> pair in WeightedNodeBuildings)
        {
            nodePairs += pair.Key.gameObject.name + ": ";
            nodePairs += pair.Value.Value + " ";
        }
        Debug.Log(nodePairs);
    }

    /// <summary>
    /// Traverse the whole graph and collect up all of the buildings into our list.
    /// </summary>
    public void FindBuildingsInGraph()
    {
        Graph graph = Graph.Instance;
        List<Node> visited = new();
        Queue<Node> frontier = new();
        frontier.Enqueue(graph.nodes[0]);

        while (frontier.Count > 0)
        { //Start from the first node and follow connections until frontier is empty
            Node node = frontier.Dequeue();
            if (node.NodeBuilding != NodeBuilding.NONE)
            { //This has a building on it, so check that its not marked VILLAGE and add it to our list
                if (node.NodeBuilding != NodeBuilding.VILLAGE)
                { //This has a proper building to track on it, so into the list it goes
                    BuildingNodes.Add(node);
                    BuildingIntPair pair = BuildingImportance.GetBuildingValue(node.NodeBuilding);
                    WeightedNodeBuildings.Add(node, new BuildingIntPair(pair.Building, pair.Value));
                }
            }
            visited.Add(node);

            foreach (Node n in node.Connections)
            {
                if (!visited.Contains(n) && !frontier.Contains(n))
                    frontier.Enqueue(n); //Only add if we haven't checked it and haven't added it
            }
            //Now loop back up and work through all of the graph
        }
    }

}
