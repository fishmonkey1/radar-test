using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Squad", menuName = "ScriptableObjects/Squad")]
public class SquadScriptableObject : ScriptableObject
{
    public List<EnemyAmount> EnemyTypes;
    public OrderWeight[] PreferredOrders;

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