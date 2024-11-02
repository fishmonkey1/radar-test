using System.Collections.Generic;
using UnityEngine;

public class Graph : MonoBehaviour
{
    static Graph instance;
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

    public void CalculateNodeDistances()
    {
        foreach (var node in nodes)
        {
            node.CalculateConnectionDistances(); //Just do this for all of the nodes
        }
    }

    public void AddNodesToGraph()
    {
        if (nodes.Count == 0)
        {
            Debug.Log("You haven't assigned a node to the graph");
            return;
        }

        Queue<Node> frontier = new();
        List<Node> visited = new();
        frontier.Enqueue(nodes[0]);

        while (frontier.Count > 0)
        {
            Node node = frontier.Dequeue();
            if (!nodes.Contains(node))
            {
                nodes.Add(node);
                Debug.Log("Added node to graph");
            }
            visited.Add(node);
            foreach (var connection in node.Connections)
            {
                if (!visited.Contains(connection))
                {
                    frontier.Enqueue(connection);
                }
            }

        }
        Debug.Log("Graph.nodes count is: " + nodes.Count);
    }

}
