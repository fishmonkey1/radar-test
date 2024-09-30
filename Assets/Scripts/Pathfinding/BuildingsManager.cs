using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BuildingsManager : MonoBehaviour
{
    public List<Node> BuildingNodes = new(); //If a node has a building, it ends up in here
    public Dictionary<Node, BuildingIntPair> WeightedNodeBuildings = new(); //Track the values of the nodes here

    public void ChangeBuildingValue(Node node, int change)
    { //Doing this so I can hook debug stuff up later if we need
        BuildingIntPair pair = WeightedNodeBuildings[node];
        pair.Value += change; 
    }

    public List<Node> GetHighestValuedBuildings(int numResults, List<NodeBuilding> exclude = null)
    {
        exclude = exclude ?? new List<NodeBuilding>(); //Initialize the list if it doesnt get passed

        List<Node> topBuildings = WeightedNodeBuildings //Linq statement
        .Where(pair => !exclude.Contains(pair.Value.Building)) //Filter out any buildings that were excluded
        .OrderByDescending(pair => pair.Value.Value) //Sort from highest at the start to lowest
        .Take(numResults) //Only grab however many results they asked for
        .Select(pair => pair.Key) //Swap the value to Node instead of BuildingIntPair
        .ToList(); //And finally make it a list

        return topBuildings;
    }

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
                    WeightedNodeBuildings.Add(node, pair);
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
