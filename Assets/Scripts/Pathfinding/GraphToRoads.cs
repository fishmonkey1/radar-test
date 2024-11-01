using System.Collections.Generic;
using UnityEngine;

public class GraphToRoads : MonoBehaviour
{

    [SerializeField]
    GameObject NodesToRoadsPrefab; //This converts the assigned nodes into the drawn roads
    GameObject RoadsParent; //For putting all the road renderers inside so they dont clutter the inspector
    List<Node> Visited = new();
    [SerializeField]
    List<NodesToRoads> RoadDraws = new(); //For viewing all of the Node draw components in the editor


    private Graph graph; //For traversing the graph from the first node and following the paths

    public struct NodePair
    {
        public Node IntersectionNode;
        public Node RoadStartNode;

        public NodePair(Node intersectionNode, Node roadStartNode)
        {
            IntersectionNode = intersectionNode;
            RoadStartNode = roadStartNode;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        DrawRoads();
    }

    public void DrawRoads()
    {
        RoadsParent = GameObject.Find("Roads");
#if UNITY_EDITOR
        if (RoadsParent != null)
            GameObject.DestroyImmediate(RoadsParent);
#endif
        if (RoadsParent != null)
        {
            GameObject.Destroy(RoadsParent); //Remove the old renderers and prep to make new ones
        }
        TraverseGraph();
        foreach(NodesToRoads draw in RoadDraws)
        {
            draw.DrawRoadsFromNodes(); //After assigning all the nodes, tell the components to actually render their paths
        }
    }

    /// <summary>
    /// Writing this out as its own separate function so that when ProcGen roads are done this can be called after they're finished. This starts from the first node and goes through each, finding ones flagged as new roads to make RoadsToNodes objects for
    /// </summary>

    public void TraverseGraph()
    {
        if (graph.nodes.Count <= 1)
        {
            Debug.Log("Not enough nodes to render roads! Add more nodes or check GraphToRoads for issues.");
            return;
        }

        graph = Graph.Instance; //Get a reference to the one and only graph in the scene
        Visited.Clear(); //I highly doubt we'll be calling this more than once, but it never hurts
        RoadDraws.Clear();

        Node firstNode = graph.nodes[0]; //Get the first one in the list which we'll assume is a root road
        Queue<NodePair> newRoadPairs = new();
        NodePair rootNode = new NodePair(null, firstNode); //The first argument is the node that serves as the intersection, which doesn't exist in the root node.
        newRoadPairs.Enqueue(rootNode);
        RoadsParent = new GameObject("Roads");
        while (newRoadPairs.Count > 0)
        { 
            //While there are still roads to make, follow the nodes until we run out of ones not flagged as new roads
            //Nodes flagged as new roads in the connections get enqueued in newRoadPairs
            //Repeat until all roads are placed

            NodePair roadStartPair = newRoadPairs.Dequeue();
            Node roadStartNode = roadStartPair.RoadStartNode;

            GameObject road = GameObject.Instantiate(NodesToRoadsPrefab, RoadsParent.transform);
            NodesToRoads roadDraw = road.GetComponent<NodesToRoads>();
            RoadDraws.Add(roadDraw); //Put the newly spawned component in the list for viewing in the editor

            if (roadStartPair.IntersectionNode != null)
            { //This means there is a node that needs to actually be the start of the road so they look like intersections
                roadDraw.AddNode(roadStartPair.IntersectionNode);
            }

            Queue<Node> remainingRoadNodes = new();
            remainingRoadNodes.Enqueue(roadStartNode);

            //Now we go down the node connections to looking for unvisited nodes and more intersections

            while (remainingRoadNodes.Count > 0)
            {
                Node node = remainingRoadNodes.Dequeue();
                if (Visited.Contains(node))
                { //We've already done stuff for this node, so lets skip it
                    //To be honest, I don't think we'll ever actually have this problem though
                    break; //Get out of the while loop and just grab the next remaining road node
                }
                roadDraw.AddNode(node); //Assign the node to the draw component
                node.SetRenderer(roadDraw); //Assign the renderer to the node too
                Visited.Add(node); //And mark the node as visited so we never add it again
                foreach (Node connectedNode in node.Connections)
                { //Check if any connected nodes are flagged as new roads (intersections)
                    if (Visited.Contains(connectedNode) && !roadDraw.ContainsNode(connectedNode))
                    { //We've added a loop, so only add the end node to render, but do not enqueue visited nodes
                        roadDraw.AddNode(connectedNode);
                    }
                    if (connectedNode.StartOfNewRoad && !Visited.Contains(connectedNode))
                    { //There's an intersection here, so get the intersection node and start of road node
                        NodePair intersectionPair = new NodePair(node, connectedNode);
                        newRoadPairs.Enqueue(intersectionPair); //Set them up to be handled on a later loop
                    }
                    else
                    {
                        if (!Visited.Contains(connectedNode))
                        {
                            remainingRoadNodes.Enqueue(connectedNode); //Prep this node for the next loop
                        }
                        //Otherwise we do nothing with this node in the connections
                    }
                }
            }

        }

    }

}
