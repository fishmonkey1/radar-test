using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The EnemyManager is getting fairly swole, so I think I'll keep all of the order selection and FSM transition stuff in here. My idea is to use the squads from the EnemyManager's list to determine what orders to pick next, and we'll track all the weights and such in here
/// </summary>
public class OrdersManager
{
    //Hey future Vicky, this may need an extra dictionary that stores the nodes so we can check if the same order has been assigned to the same location, instead 
    Dictionary<Orders, List<EnemySquad>> SquadsByOrders = new(); //If we assign an order, add the squad to its list
    Dictionary<Node, List<OrdersSquadPair>> NodesByOrders = new();
    //TODO: Future Vicky, this PatrolLength shouldn't just be a magic number!
    public int PatrolLength = 2; //Just a there and back deal for now, this needs to be assigned from the inspector later

    Dictionary<Orders, int> HardcodedOrderWeights = new()
    {
        { Orders.PATROL, 1 }, //Weight equally with guarding
        { Orders.GUARD, 1 }, //Weighted the same as patrol so that squad order preferances break ties
        { Orders.ASSAULT_STANDBY, 0 }, //Less important to assign to new squads unless they prefer being assault squads
    };

    /// <summary>
    /// Find all enemies that exist and assign orders to them.
    /// </summary>
    public void StartingOrders()
    { //We assign all of the orders we need in here and place the enemies at their assigned node
        foreach (EnemySquad squad in EnemyManager.Instance.AllSquads)
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
            squad.SetOrder(order); //Assign the order to the squad, firing its delegate if anyone is listening
        }
    }

    /// <summary>
    /// Fill in the OrderContext so the correct function is run.
    /// </summary>
    /// <param name="order"></param>
    /// <returns></returns>
    OrderContext PopulateOrderContext(OrderContext order)
    {
        //Stubbed this out for now, this should basically be a a switch statement
        
        switch (order.Order)
        {
            case Orders.PATROL:
                PopulatePatrol(order as PatrolOrder);
                break;
            case Orders.GUARD:
                PopulateGuard(order as GuardOrder);
                break;
            case Orders.ASSAULT_STANDBY:
                PopulateAssault(order as AssaultStandbyOrder);
                break;
            default:
                Debug.Log($"Tried to populate order {nameof(order)} but this order type is not implemented");
                return null;
        }

        return order;
    }

    OrderContext PopulateAssault(AssaultStandbyOrder assault)
    {
        //TODO: Future Vicky should change this so that only certain buildings are valid candidates for hosting an assault squad
        List<Node> buildings = EnemyManager.Instance.BuildingsManager.GetHighestValuedBuildings(1); //We wait at this building for assault
        assault.Node = buildings[0]; //Get the one node we asked for
        //TODO: Future Vicky should consider using the list of nodes by orders to affect this instead of changing the base dictionary
        EnemyManager.Instance.BuildingsManager.ChangeBuildingValue(assault.Node, -1); //One squad has this order, so we remove some importance.
        return assault; //And send it back
    }

    OrderContext PopulateGuard(GuardOrder guard)
    {
        List<Node> buildings = EnemyManager.Instance.BuildingsManager.GetHighestValuedBuildings(1); //We can only guard one spot at a time
        guard.Node = buildings[0]; //Get the first one which should be the only one
        //At the moment there isn't anything else to do with this one
        return guard;
    }

    OrderContext PopulatePatrol(PatrolOrder patrol)
    {
        //We need to get our highest rated pair of buildings to try to build a patrol out of
        BuildingsManager buildingManager = EnemyManager.Instance.BuildingsManager;
        List<Node> buildings = buildingManager.GetHighestValuedBuildings(PatrolLength); //No need for excluding buildings now
        Debug.Log("Testing WeightedBuildingNodes list after getting HighestValuedBuildings");
        buildingManager.PrintWeightedNodes();
        string listbuildings = ""; //For quick debugging of the node names
        foreach(Node building in buildings)
        {
            listbuildings += building.gameObject.name + " ";
        }

        Debug.Log($"Contents of HighestValuedBuildings is: {listbuildings}");
        

        //Now we need to set up our OrderContext with some of the data

        patrol.Node = buildings[0]; //Enemies spawn on this node, so we assign it to the start of the patrol
        //Now we need to get a path from the first node to the second node and assign that to patrol.Nodes
        List<Node> path = AStar.GetPath(patrol.Node, buildings[1]);
        patrol.Nodes = path; //Assign the path we found to the patrol
        patrol.Looping = true; //I'm just gonna set them all to looping for right now
        Debug.Log($"Created a patrol path. Contents of path are " + AStar.PrintPath(path));
        //Since we assigned an order to these nodes, we should reduce their weights
        //I only have two buildings for now, so I've commented out the second building getting decremented
        buildingManager.ChangeBuildingValue(patrol.Node, -1); //Reduce the weight since a squad is assigned
        //buildingManager.ChangeBuildingValue(buildings[1], -1);
        return patrol;
    }

}

public class OrdersSquadPair
{
    public Orders Order;
    public EnemySquad Squad;
}