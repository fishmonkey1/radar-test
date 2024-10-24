using System.Collections;
using UnityEngine;
using PathCreation;

public class NavigateRoads : Navigation
{

    NodesToRoads roadNodes; //The road we're on and trying to follow
    PathCreator roadPath;
    Node nextNode; //The next node we're trying to reach
    [SerializeField] float rangeCheck = 0.5f; //How far the transform can be before we count it as reached
    [SerializeField] float rangeInterval = 0.5f; //How often we check to see if we're near the node
    PatrolOrder patrolOrder; //Store a reference to this since I expect most usage of this script to be from patrols
    Steering steer;

    public void Initialize()
    {
        owner = GetComponent<Enemy>(); //Cache the enemy so we can pull their speeds and such
        squad = owner.Squad; //Nab the squad so we can follow our orders
        active = true; //I'm just assuming that these won't need to be turned off for now

        roadNodes = GetComponent<Enemy>().NearNode.NodeRenderer; //Find the renderer from the node we've been teleported to by the spawner
        roadPath = roadNodes.GetPathCreator();

        if (squad.OrderContext.Order == Orders.PATROL)
        { //We should grab the path since we're likely doing a patrol
            //Future Vicky, I'm pretty sure the path is already populated from the OrdersManager
            patrolOrder = squad.OrderContext as PatrolOrder;
            nextNode = patrolOrder.Nodes[1]; //Assign the second node in the list to be the first to go towards
        }
        //Now we need to rotate our unit to face the node it's going to
        transform.LookAt(nextNode.transform.position); //Point towards the node
        steer = GetComponent<Steering>();
        StartCoroutine(CheckDistanceToNextNode()); //Start the distance check
    }

    void Update()
    {
        //Get the point the truck is going to move to
        Vector3 aheadPoint = transform.position + transform.forward * owner.MaxSpeed * Time.deltaTime;
        //Now we find the closest point on the path to our aheadPoint
        //If we changed road renderers, the unit should be facing the next node, which should prevent wrong turns
        Vector3 pathPoint = roadPath.path.GetClosestPointOnPath(aheadPoint);
        transform.LookAt(pathPoint); //Orientate to face the point on the path, then move towards it
        transform.position += transform.forward * owner.MaxSpeed * Time.deltaTime;
        //steer.UpdateSteering(pathPoint);
    }

    public IEnumerator CheckDistanceToNextNode()
    {
        while (true)
        {
            float distance = Vector3.Distance(transform.position, nextNode.transform.position);

            //Debug.Log("Checking distance to nodes");
            if (distance <= rangeCheck)
            {
                nextNode = patrolOrder.GetNextNode(nextNode);
                Debug.Log("Getting next node in path. Next node is " + nextNode.name);
                if (nextNode.NodeRenderer != roadNodes)
                { //This means we're at an intersection and need to continue down a new road
                    roadNodes = nextNode.NodeRenderer;
                    roadPath = roadNodes.GetPathCreator();
                    Debug.Log("Next node has a different renderer, reassigning...");
                }
                //Lets try rotating the model to face the node, so the next update cycle starts with LookAt(nextNode)
                transform.LookAt(nextNode.transform.position);
            }

            yield return new WaitForSeconds(rangeInterval);
        }
    }
}
