using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public List<Node> Connections = new(); //Other nodes reachable from this one
    public bool StartOfNewRoad; //If this needs to have another road renderer used
}
