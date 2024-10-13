using PathCreation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Enemy))] //This should only go on enemies
public class NavigateRoads : Navigation
{
    public float MoveSpeed;
    NodesToRoads roadNodes; //The road we're on and trying to follow
    PathCreator roadPath;
    List<Node> path; //Contains all the road nodes we want to travel through to get to Enemy.targetNode
    Node nextNode; //The next node we're trying to reach
    [SerializeField] float rangeCheck = 0.5f; //How far the transform can be before we count it as reached
    [SerializeField] float rangeInterval = 0.5f; //How often we check to see if we're near the node
    EndOfPathInstruction endOfPathInstruction;

    float distanceTravelled = 0f;

    public void Initialize()
    {
        owner = GetComponent<Enemy>(); //Cache the enemy so we can pull their speeds and such
        squad = owner.Squad; //Nab the squad so we can follow our orders
        active = true; //I'm just assuming that these won't need to be turned off for now

        //Now we need to locate our NodesToRoads object
        roadNodes = GetComponent<Enemy>().NearNode.NodeRenderer; //Find the renderer from the node we've been teleported to by the spawner
        roadPath = roadNodes.GetPathCreator();
        if (squad.OrderContext.Order == Orders.PATROL)
        { //We should grab the path since we're doing a patrol
            //Future Vicky, I'm pretty sure the path is already populated from the OrdersManager
            PatrolOrder patrol = squad.OrderContext as PatrolOrder;
            path = patrol.Nodes;
            nextNode = patrol.Node; //Assign the first node as our target and let the update function mark it as reached
            if (patrol.Looping) //If we loop then actually set the instruction to reverse
                endOfPathInstruction = EndOfPathInstruction.Reverse;
            else
                endOfPathInstruction = EndOfPathInstruction.Stop;
        }
        StartCoroutine(CheckDistanceToNode()); //Start the distance check
    }

    // Update is called once per frame
    void Update()
    {
        if (active)
        {
            distanceTravelled += MoveSpeed * Time.deltaTime;
            transform.position = roadPath.path.GetPointAtDistance(distanceTravelled, endOfPathInstruction);
            transform.rotation = roadPath.path.GetRotationAtDistance(distanceTravelled, endOfPathInstruction);
        }
    }

    IEnumerator CheckDistanceToNode()
    { //Gets checked every rangeInterval seconds and updates the next node
        while (true)
        {
            float distance = Vector3.Distance(transform.position, nextNode.transform.position);

            //Debug.Log("Checking distance to nodes");
            if (distance <= rangeCheck)
            {
                PatrolOrder patrol = squad.OrderContext as PatrolOrder;
                nextNode = patrol.GetNextNode(nextNode);
                Debug.Log("Getting next node in path. Next node is " + nextNode.name);
                if (nextNode.NodeRenderer != roadNodes)
                { //This means we're at an intersection and need to continue down a new road
                    roadNodes = nextNode.NodeRenderer;
                    roadPath = roadNodes.GetPathCreator();
                    distanceTravelled = 0;
                    Debug.Log("Next node has a different renderer, reassigning...");
                }
            }

            yield return new WaitForSeconds(rangeInterval);
        }
    }
}
