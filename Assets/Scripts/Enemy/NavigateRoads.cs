using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Enemy)] //This should only go on enemies
public class NavigateRoads : MonoBehaviour
{
    EnemySquad squad;
    Enemy owner;
    [SerializeField] bool active; //If the script is inactive, we ignore all our update stuff
    NodesToRoads roadNodes; //The road we're on and trying to follow

    void Awake()
    {
        owner = GetComponent<Enemy>(); //Cache the enemy so we can pull their speeds and such
        squad = owner.Squad; //Nab the squad so we can follow our orders
        active = true; //I'm just assuming that these won't need to be turned off for now

        //Now we need to locate our NodesToRoads object
        roadNodes = GetComponent<Enemy>().NearNode.NodeRenderer; //Find the renderer from the node we've been teleported to by the spawner
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
