using System.Collections.Generic;
using UnityEngine;

public class EnemySquad
{
    List<Enemy> SquadMembers = new(); //All the enemies that compose this squad

    Node TargetNode; //The node the squad is meant to go to
    Node NearestNode; //The node the squad members are closest to
    
    //I need some way of giving squads orders here... enum for now
    Orders CurrentOrder = Orders.IDLE; //Order the squad is currently doing
    public OrderContext OrderContext { get; protected set; } //This holds the extra data that the order has been assigned, currently NULL for the idle order
    public delegate void OrderChangedDelegate(OrderContext order); //Enemy listens to this delegate
    public OrderChangedDelegate OnOrderChanged { get; protected set; } //Can fetch, but can't overwrite

    public EnemySquad()
    {
        OnOrderChanged = new(LogOrderChanged);
    }

    public void SetOrder(OrderContext orderContext)
    { //Assigning the new order to the squad.
        OrderContext = orderContext;
        CurrentOrder = orderContext.Order;
        if (OnOrderChanged != null)
        {
            OnOrderChanged.Invoke(orderContext);
        }
    }

    public void AddEnemy(GameObject enemy)
    {
        SquadMembers.Add(enemy.GetComponent<Enemy>()); //I'll handle error checking for this later
    }

    public void AddEnemy(Enemy enemy)
    { //Override for above, using the enemy script instead
        SquadMembers.Add(enemy);
    }

    void LogOrderChanged(OrderContext order)
    {
        Debug.Log("Enemy Squad had it's orders changed to " + nameof(order.Order));
    }
}
