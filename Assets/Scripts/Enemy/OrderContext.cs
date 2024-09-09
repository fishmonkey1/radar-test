using Org.BouncyCastle.Asn1.Mozilla;
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
