using System.Collections.Generic;

public class EnemySquad
{
    List<Enemy> SquadMembers = new(); //All the enemies that compose this squad

    Node TargetNode; //The node the squad is meant to go to
    Node NearestNode; //The node the squad members are closest to

    //I need some way of giving squads orders here... enum for now
    Orders CurrentOrder = Orders.IDLE; //Order the squad is currently doing
}
