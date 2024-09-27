using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The EnemyManager is getting fairly swole, so I think I'll keep all of the order selection and FSM transition stuff in here. My idea is to use the squads from the EnemyManager's list to determine what orders to pick next, and we'll track all the weights and such in here
/// </summary>
public class OrdersManager
{
    //Hey future Vicky, this may need an extra dictionary that stores the nodes so we can check if the same order has been assigned to the same location, instead 
    Dictionary<Orders, List<EnemySquad>> SquadsByOrders = new(); //If we assign an order, add the squad to its list

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
            Dictionary<Orders, int> Weights = new(HardcodedOrderWeights);
            if (squad.OrderWeights.Length > 0)
            { //There are actually preferred orders for this squad
                //We need to find the highest weighted order, so we need to make a new Dictionary to check
                foreach (OrderWeight weightedOrder in  squad.OrderWeights)
                { //Now adjust the new weights based on the squad's preferrance
                    if (Weights.ContainsKey(weightedOrder.Order))
                    { //We adjust the value before we pick
                        Weights[weightedOrder.Order] += weightedOrder.Weight;
                    }
                }
            }
            //Now that the orders are weighted, it's time to pick the highest valued one
            (Orders, int) highestValuePair =  new (Orders.IDLE, -999); //making this a tuple for now
            foreach( KeyValuePair<Orders, int> pair in Weights)
            { //Lets find the highest one and assign that order to highestValue
                if (pair.Value > highestValuePair.Item2)
                {
                    highestValuePair.Item1 = pair.Key;
                    highestValuePair.Item2 = pair.Value;
                }
            }
            //Now we have the highest value order done. Lets build a context and do stuff based on which order it is
            OrderContext order = OrderContext.CreateOrder(highestValuePair.Item1);

            order = PopulateOrderContext(order);

        }
    }

    OrderContext PopulateOrderContext(OrderContext order)
    {
        //Stubbed this out for now, this should basically be a a switch statement

        return order;
    }
}
