using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds all of the nodes that make up the roads of a map. At least one node needs to be added to the graph in order for the graph to discover all connected nodes.
/// </summary>
public class Graph : MonoBehaviour
{
    static Graph instance;
    /// <summary>
    /// There should only be one graph per gameplay scene.
    /// </summary>
    public static Graph Instance
    { get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<Graph>();
            }
            return instance;
        }
    }
    public List<Node> nodes;

    void Awake()
    {
        CalculateNodeDistances();
    }

    /// <summary>
    /// Determine how far all of the nodes are from their neighbors.
    /// </summary>
    public void CalculateNodeDistances()
    {
        foreach (var node in nodes)
        {
            node.CalculateConnectionDistances(); //Just do this for all of the nodes
        }
    }

}
