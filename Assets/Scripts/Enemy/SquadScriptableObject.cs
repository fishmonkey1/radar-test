using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Squad", menuName = "ScriptableObjects/Squad")]
public class SquadScriptableObject : ScriptableObject
{

    [System.Serializable]
    public struct EnemyAmount
    {
        public GameObject EnemyPrefab;
        public int MinNumber, MaxNumber;
    }

    public List<EnemyAmount> EnemyTypes;

}
