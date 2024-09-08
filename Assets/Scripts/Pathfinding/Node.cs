using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public List<Node> Connections = new(); //Other nodes reachable from this one
    public bool StartOfNewRoad; //If this needs to have another road renderer used
    //The AI uses this to determine if this node should be weighted higher for pathfinding, for use with highways for example
    public uint NodeImportance;
    public NodeTraversal NodeTraversal = NodeTraversal.NOT_SET;
}
