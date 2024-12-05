using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Concrete order types inherit from this context, and the base class is capable of producing its child orders.
/// </summary>
[Serializable]
public abstract class OrderContext
{
    public Orders Order { get; protected set; } //The order this context class goes to
    public Node Node; //The node the order targets, used for everything but patrol

    public virtual void FinishOrder() { } //For use in children classes
    public virtual bool CheckOrderFinished() { return false; }

    /// <summary>
    /// Get an order based on the OrderType.
    /// </summary>
    /// <param name="order"></param>
    /// <returns></returns>
    /// <exception cref="System.ArgumentException"></exception>
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

/// <summary>
/// Information for all of the nodes this squad will follow.
/// </summary>
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

    /// <summary>
    /// Return the next node in the list to patrol along.
    /// </summary>
    /// <param name="currentNode"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
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

/// <summary>
/// Stub for searching for spotted players.
/// </summary>
public class InvestigateOrder : OrderContext
{
    //I'll need stuff about the search pattern somewhere in here, so I'll leave this one as a stub until I figure out what I'm doing... :c

    public InvestigateOrder()
    {
        Order = Orders.INVESTIGATE;
    }
}

/// <summary>
/// Stub for defending groups moving to a new location.
/// </summary>
public class RepositionOrder : OrderContext
{
    //This one is also going to remain a stub for now until I decide if I'm going to have messages in the orders or in the containing class. I'm personally leaning towards having the manager sort it out, but we'll see

    public RepositionOrder()
    {
        Order = Orders.REPOSITION;
    }
}

/// <summary>
/// Stub for when enemies arrive at a guard location.
/// </summary>
public class GuardOrder : OrderContext
{
    public GuardOrder()
    {
        Order = Orders.GUARD;
    }
}

/// <summary>
/// Stub for when enemies are idling at a location waiting to respond to player spottings.
/// </summary>
public class AssaultStandbyOrder : OrderContext
{
    public AssaultStandbyOrder()
    {
        Order = Orders.ASSAULT;
    }
}

/// <summary>
/// Stub for when enemies are order from assault standby to engage players.
/// </summary>
public class AssaultOrder : OrderContext
{
    public AssaultOrder()
    {
        Order = Orders.ASSAULT;
    }
}

/// <summary>
/// Stub for when enemies have taken losses and try to move to a safe node.
/// </summary>
public class RetreatOrder : OrderContext
{
    public RetreatOrder()
    {
        Order = Orders.RETREAT;
    }
}

/// <summary>
/// Stub for when enemies have lost the player or are called back to a location.
/// </summary>
public class FallbackOrder : OrderContext
{
    public FallbackOrder()
    {
        Order = Orders.FALLBACK;
    }
}