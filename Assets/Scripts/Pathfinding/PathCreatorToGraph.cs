using PathCreation;
using PathCreation.Examples;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RoadMeshCreator))]
public class PathCreatorToGraph : MonoBehaviour
{
    //The PathCreator can't handle branches, so we can only render one length of path at a time.
    //I'll be experimenting with this so you don't have to set it up manually for the graph

    [SerializeField]
    List<Node> nodes = new List<Node>();
    [SerializeField]
    RoadMeshCreator RoadMeshCreator; //Edit the values here 

    // Start is called before the first frame update
    void Awake()
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
    }
}
