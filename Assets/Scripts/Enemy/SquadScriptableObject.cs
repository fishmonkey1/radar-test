using HorniTank;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Allows designers to specify prebuilt squads with either static or variable numbers of enemies in it.
/// </summary>
[CreateAssetMenu(fileName = "Squad", menuName = "ScriptableObjects/Squad")]
public class SquadScriptableObject : ScriptableObject
{
    /// <summary>
    /// All of the enemies we'll be spawning, and in what amounts
    /// </summary>
    public List<EnemyAmount> EnemyTypes;
    /// <summary>
    /// The orders that this squad type gets priority on
    /// </summary>
    public OrderWeight[] PreferredOrders;
    /// <summary>
    /// Set which team this squad is on by default
    /// </summary>
    public TeamInfo SquadTeam;
}

[System.Serializable]
public struct EnemyAmount
{
    public GameObject EnemyPrefab;
    public int MinNumber, MaxNumber;

    public int RollNumber()
    {
        return Random.Range(MinNumber, MaxNumber);
    }
}

[System.Serializable]
public class OrderWeight
{
    public Orders Order;
    public int Weight;

    public OrderWeight() { }

    public OrderWeight(Orders order, int weight)
    {
        Order = order;
        Weight = weight;
    }

}