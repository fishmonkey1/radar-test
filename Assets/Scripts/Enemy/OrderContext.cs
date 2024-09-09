using System;
using System.Collections.Generic;

public abstract class OrderContext
{
    public Orders Order { get; protected set; } //The order this context class goes to

    public virtual void FinishOrder() { } //For use in children classes
    public virtual bool CheckOrderFinished() { return false; }

    public static OrderContext CreateOrder(Orders order)
    {
        return order switch
        {
            Orders.PATROL => new PatrolOrder(),
            Orders.INVESTIGATE => new InvestigateOrder(),
            Orders.REPOSITION => new RepositionOrder(),
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

        return Nodes[index + 1];
    }

}

public class InvestigateOrder : OrderContext
{
    public Node Node;
    //I'll need stuff about the search pattern somewhere in here, so I'll leave this one as a stub until I figure out what I'm doing... :c

    public InvestigateOrder()
    {
        Order = Orders.INVESTIGATE;
    }
}

public class RepositionOrder : OrderContext
{
    //This one is also going to remain a stub for now until I decide if I'm going to have messages in the orders or in the containing class. I'm personally leaning towards having the manager sort it out, but we'll see
    public Node Node; //The node the unit is going to

    public RepositionOrder()
    {
        Order = Orders.REPOSITION;
    }
}