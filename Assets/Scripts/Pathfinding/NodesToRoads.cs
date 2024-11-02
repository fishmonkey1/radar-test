using PathCreation;
using PathCreation.Examples;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RoadMeshCreator))]
public class NodesToRoads : MonoBehaviour
{
    //The PathCreator can't handle branches, so we can only render one length of path at a time.
    //I'll be experimenting with this so you don't have to set it up manually for the graph

    [SerializeField]
    List<Node> nodes = new List<Node>();
    [SerializeField]
    RoadMeshCreator RoadMeshCreator; //Edit the values here

    public void DrawRoadsFromNodes()
    {
        if (RoadMeshCreator == null)
            RoadMeshCreator = GetComponent<RoadMeshCreator>();

        List<Transform> nodeTransforms = new List<Transform>();
        foreach(var node in nodes)
        { //Make a list out of the nodes' positions in the scene
            nodeTransforms.Add(node.transform);
        }

        BezierPath path = new BezierPath(nodeTransforms, false, space: PathSpace.xyz);
        RoadMeshCreator.pathCreator.bezierPath = path; //Assign the nodes from the scene to the road renderer
        RoadMeshCreator.textureTiling = 1; //Attempt at hard coding this value before drawing
        RoadMeshCreator.TriggerUpdate(); //Make the road mesh creator actually draw out the road
    }

    public void AddNode(Node node)
    {
        nodes.Add(node); //Das it, mane
    }

    public bool ContainsNode(Node node)
    {
        return nodes.Contains(node);
    }

    public PathCreator GetPathCreator()
    {
        return RoadMeshCreator.pathCreator;
    }
}
