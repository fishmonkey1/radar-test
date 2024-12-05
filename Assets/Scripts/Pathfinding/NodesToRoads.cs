using PathCreation;
using PathCreation.Examples;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Turns a path of <see cref="Node"/> objects into a road mesh.
/// </summary>
[RequireComponent(typeof(RoadMeshCreator))]
public class NodesToRoads : MonoBehaviour
{
    //The PathCreator can't handle branches, so we can only render one length of path at a time.
    //I'll be experimenting with this so you don't have to set it up manually for the graph

    [SerializeField]
    List<Node> nodes = new List<Node>();
    [SerializeField]
    RoadMeshCreator RoadMeshCreator; //Edit the values here

    /// <summary>
    /// Create the mesh creator and assign it all the node info.
    /// </summary>
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

    /// <summary>
    /// Add a node to the renderer's path.
    /// </summary>
    /// <param name="node"></param>
    public void AddNode(Node node)
    {
        nodes.Add(node); //Das it, mane
    }

    /// <summary>
    /// Ensure the renderer's list doesn't have duplicates.
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public bool ContainsNode(Node node)
    {
        return nodes.Contains(node);
    }

    /// <summary>
    /// For tweaking the meshes the creator builds.
    /// </summary>
    /// <returns></returns>
    public PathCreator GetPathCreator()
    {
        return RoadMeshCreator.pathCreator;
    }
}
