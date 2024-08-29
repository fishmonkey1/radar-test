using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public List<Node> Connections = new(); //Other nodes reachable from this one
}
