using System.Collections.Generic;
using UnityEngine;

public class Graph : MonoBehaviour
{
    public static Graph Instance;
    public List<Node> nodes;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogWarning("You have multiple graph components in the scene. Delete graph named " + gameObject.name);
        }
        Instance = this;
    }
}
