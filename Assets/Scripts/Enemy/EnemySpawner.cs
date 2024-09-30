using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemySpawner
{
    public List<SquadSO_Pair> SquadTemplates = new();

    public void CreateSquads()
    {
        foreach (SquadSO_Pair pair in SquadTemplates)
        {
            SquadScriptableObject SquadSO = pair.SquadSO;
            EnemySquad squad = new EnemySquad();

            foreach (EnemyAmount enemy in SquadSO.EnemyTypes)
            {
                int num = enemy.RollNumber(); //Determine the number of enemies in this squad type
                for (int i = 0; i < num; i++)
                { //Now spawn one of these enemies, assign it to its squad, and put it in the EnemyManager's list
                    GameObject newEnemy = GameObject.Instantiate(enemy.EnemyPrefab); //We won't do anything about its position until orders are assigned
                    EnemyManager.Instance.AllEnemies.Add(newEnemy.GetComponent<Enemy>());
                    squad.AddEnemy(newEnemy); //Track all instantiated enemies in the manager
                    squad.OrderWeights = SquadSO.PreferredOrders; //Copy the preferred order array over to the new squad
                }
            }
            EnemyManager.Instance.AllSquads.Add(squad); //Put the new squad in the list
        }
    }

}

[System.Serializable]
public class SquadSO_Pair
{
    public SquadScriptableObject SquadSO;
    public int Amount;
}