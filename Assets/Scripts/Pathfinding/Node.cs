using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public Transform ScenePosition; //Where the node is in the scene
    public List<Node> Connections = new(); //Other nodes reachable from this one
}
