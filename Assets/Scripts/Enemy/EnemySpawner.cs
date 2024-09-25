using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemySpawner
{
    public List<SquadSO_Pair> SquadTemplates = new();


}

[System.Serializable]
public class SquadSO_Pair
{
    public SquadScriptableObject SquadSO;
    public int Amount;
}