using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public List<Node> Connections = new(); //Other nodes reachable from this one
    public bool StartOfNewRoad; //If this needs to have another road renderer used
    //The AI uses this to determine if this node should be weighted higher for pathfinding, for use with highways for example
    public uint NodeImportance; //The level of importance paid by the enemy AI to this node
    public NodeTraversal NodeTraversal = NodeTraversal.NOT_SET;
    public NodeBuilding NodeBuilding = NodeBuilding.NONE; //Change the node to indicate that an important structure is nearby
    public VillageInfo village;

    public void Start()
    {
        if (!Graph.Instance.nodes.Contains(this))
            Graph.Instance.nodes.Add(this);
    }
}
