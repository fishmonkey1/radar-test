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
}
