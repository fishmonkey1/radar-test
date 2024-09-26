using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The EnemyManager is getting fairly swole, so I think I'll keep all of the order selection and FSM transition stuff in here. My idea is to use the squads from the EnemyManager's list to determine what orders to pick next, and we'll track all the weights and such in here
/// </summary>
public class OrdersManager
{

    Dictionary<Orders, int> HardcodedOrderWeights = new()
    {
        { Orders.PATROL, 1 }, //Weight equally with guarding
        { Orders.GUARD, 1 }, //Weighted the same as patrol so that squad order preferances break ties
        { Orders.ASSAULT_STANDBY, 0 }, //Less important to assign to new squads unless they prefer being assault squads
    };

    public void StartingOrders()
    { //We assign all of the orders we need in here and place the enemies at their assigned node
        foreach (EnemySquad squad in EnemyManager.Singleton.AllSquads)
        {

        }
    }
}
