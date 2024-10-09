using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public List<Node> Connections = new(); //Other nodes reachable from this one
    public Dictionary<Node, float> ConnectionDistances = new();
    public bool StartOfNewRoad; //If this needs to have another road renderer used
    //The AI uses this to determine if this node should be weighted higher for pathfinding, for use with highways for example
    public uint NodeImportance; //The level of importance paid by the enemy AI to this node
    public NodeTraversal NodeTraversal = NodeTraversal.NOT_SET;
    public NodeBuilding NodeBuilding = NodeBuilding.NONE; //Change the node to indicate that an important structure is nearby
    public VillageInfo village;
    public NodesToRoads NodeRenderer;

    public void Start()
    {
        if (!Graph.Instance.nodes.Contains(this))
            Graph.Instance.nodes.Add(this);
    }

    public void SetRenderer(NodesToRoads renderer)
    {
        NodeRenderer = renderer; //Once the graph has had the roads built, the renderer gets assigned to it
    }

    public void CalculateConnectionDistances()
    {
        foreach (var node in Connections)
        {
            //Get the nodes' locations in the world
            Transform here = this.transform;
            Transform there = node.transform;
            float distance = Vector3.Distance(here.position, there.position);
            if (!ConnectionDistances.ContainsKey(node))
            {
                ConnectionDistances.Add(node, distance);
            }
        }
    }

}
