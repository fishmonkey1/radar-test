using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class OrderContext
{
    public Orders Order { get; protected set; } //The order this context class goes to
    public Node Node; //The node the order targets, used for everything but patrol

    public virtual void FinishOrder() { } //For use in children classes
    public virtual bool CheckOrderFinished() { return false; }

    public static OrderContext CreateOrder(Orders order)
    {
        return order switch
        {
            Orders.PATROL => new PatrolOrder(),
            Orders.INVESTIGATE => new InvestigateOrder(),
            Orders.REPOSITION => new RepositionOrder(),
            Orders.GUARD => new GuardOrder(),
            Orders.ASSAULT_STANDBY => new AssaultStandbyOrder(),
            Orders.ASSAULT => new AssaultOrder(),
            Orders.RETREAT => new RetreatOrder(),
            Orders.FALLBACK => new FallbackOrder(),
            _ => throw new System.ArgumentException($"Order type {nameof(order)} has not be implemented or does not exist")
        };
    }
}

public class PatrolOrder : OrderContext
{
    //Holds all of the nodes the squad wants to visit
    public List<Node> Nodes = new();
    public bool Looping; //If true then after completing the order the patrol continue in reverse, looping

    public PatrolOrder() { Order = Orders.PATROL; }

    public override void FinishOrder()
    {
        if (Looping)
        { //If we're supposed to loop this patrol then reverse the list for the next use
            Nodes.Reverse();
            Debug.Log("Unit has reached end of looping patrol, reversing nodes.");
        }
    }

    public Node GetNextNode(Node currentNode)
    {
        int index = Nodes.IndexOf(currentNode);

        if (index == -1)
        {
            throw new ArgumentException("The current node is not in the list.", nameof(currentNode));
        }

        if (index == Nodes.Count - 1)
        {
            FinishOrder();
            if (Looping)
                return Nodes[0];
            else
                return null; // Returning null as a signal that the patrol is finished
        }

        Debug.Log($"Fetching next node in patrol. Index of current node is {index} out of path count of {Nodes.Count}");

        return Nodes[index + 1];
    }

}

public class InvestigateOrder : OrderContext
{
    //I'll need stuff about the search pattern somewhere in here, so I'll leave this one as a stub until I figure out what I'm doing... :c

    public InvestigateOrder()
    {
        Order = Orders.INVESTIGATE;
    }
}

public class RepositionOrder : OrderContext
{
    //This one is also going to remain a stub for now until I decide if I'm going to have messages in the orders or in the containing class. I'm personally leaning towards having the manager sort it out, but we'll see

    public RepositionOrder()
    {
        Order = Orders.REPOSITION;
    }
}

public class GuardOrder : OrderContext
{
    public GuardOrder()
    {
        Order = Orders.GUARD;
    }
}

public class AssaultStandbyOrder : OrderContext
{
    public AssaultStandbyOrder()
    {
        Order = Orders.ASSAULT;
    }
}

public class AssaultOrder : OrderContext
{
    public AssaultOrder()
    {
        Order = Orders.ASSAULT;
    }
}

public class RetreatOrder : OrderContext
{
    public RetreatOrder()
    {
        Order = Orders.RETREAT;
    }
}

public class FallbackOrder : OrderContext
{
    public FallbackOrder()
    {
        Order = Orders.FALLBACK;
    }
}