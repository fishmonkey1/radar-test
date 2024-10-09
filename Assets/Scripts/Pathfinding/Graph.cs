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

}
