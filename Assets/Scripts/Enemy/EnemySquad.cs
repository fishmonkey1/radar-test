using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Information about who is in an enemy squad and what orders they are following.
/// </summary>
[System.Serializable]
public class EnemySquad
{
    List<Enemy> SquadMembers = new(); //All the enemies that compose this squad

    Node TargetNode; //The node the squad is meant to go to
    Node NearestNode; //The node the squad members are closest to

    //I need some way of giving squads orders here... enum for now
    public OrderWeight[] OrderWeights;
    Orders CurrentOrder = Orders.IDLE; //Order the squad is currently doing
    public OrderContext OrderContext { get; protected set; } //This holds the extra data that the order has been assigned, currently NULL for the idle order
    public delegate void OrderChangedDelegate(OrderContext order); //Enemy listens to this delegate
    public OrderChangedDelegate OnOrderChanged { get; protected set; } //Can fetch, but can't overwrite

    public EnemySquad()
    {
        OnOrderChanged = new(LogOrderChanged);
    }

    /// <summary>
    /// Assign an order to the squad.
    /// </summary>
    /// <param name="orderContext"></param>
    public void SetOrder(OrderContext orderContext)
    { //Assigning the new order to the squad.
        OrderContext = orderContext;
        CurrentOrder = orderContext.Order;
        TargetNode = orderContext.Node;
        if (OnOrderChanged != null)
        {
            OnOrderChanged.Invoke(orderContext);
        }
    }

    /// <summary>
    /// When enemies get spawned for the squad, this adds them in.
    /// </summary>
    /// <param name="Enemy"></param>
    public void AddEnemy(GameObject Enemy)
    {
        Enemy enemy = Enemy.GetComponent<Enemy>();
        enemy.Squad = this;
        SquadMembers.Add(enemy); //I'll handle error checking for this later
    }

    public void AddEnemy(Enemy Enemy)
    { //Override for above, using the enemy script instead
        Enemy.Squad = this;
        SquadMembers.Add(Enemy);
    }

    /// <summary>
    /// After all enemies have spawned and received orders, they get teleported to their starting location.
    /// </summary>
    /// <param name="location"></param>
    public void TeleportAll(Node location)
    {
        foreach (Enemy enemy in SquadMembers)
        {
            enemy.transform.position = location.transform.position; //Set the enemy to be positioned at the starting node
            enemy.NearNode = location;
            //And lets set up the enemy's navigation while we're here too
            NavigateRoads navigate = enemy.GetComponent<NavigateRoads>();
            navigate.Initialize(); //Kick things off
        }
        NearestNode = location;
        
    }

    void LogOrderChanged(OrderContext order)
    {
        Debug.Log("Enemy Squad had it's orders changed to " + order.Order);
    }
}
