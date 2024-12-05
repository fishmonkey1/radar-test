using System.Collections.Generic;
using UnityEngine;
using Mirror;

/// <summary>
/// Tracks the Alert and Suspicion Levels of the enemy, and tracks what ongoing investigations are happening.
/// </summary>
public class EnemyManager : NetworkBehaviour
{

    static EnemyManager instance; //Lazy awake singleton pattern because lazy rn
    public static EnemyManager Instance => instance;

    public BuildingsManager BuildingsManager; //For grabbing map buildings and checking weights
    public OrdersManager OrdersManager = new();

    public AlertSuspicionLevel AlertLevel = new AlertSuspicionLevel();

    /// <summary>
    /// If a player has been spotted, this is true. Could potentially raise the alert level if spotted again
    /// </summary>
    public bool Investigating { get; private set; }
    /// <summary>
    /// How long the player should avoid enemies for the investigation to end only in suspicion
    /// </summary>
    public float InvestigateTimeLeft { get; private set; }
    public delegate void OnInvestigateChanged(bool isInvestigating);
    public OnInvestigateChanged InvestigateChanged;

    [System.Serializable]
    public struct Vector3FloatPair
    { 
        public float number;
        public Vector3 vector;
    }
    //Stores where the player was seen and how much time the search has left
    public List<Vector3FloatPair> PlayerSpottings {  get; private set; }

    public List<Enemy> AllEnemies = new();
    public delegate GameObject OnEnemySpawn(Enemy enemy);
    public OnEnemySpawn OnEnemySpawned;

    public List<EnemySquad> AllSquads = new(); //Holds all of the squads that have been made
    public List<SquadSO_Pair> SquadTemplates = new();

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Debug.Log("You have two EnemyManagers in the scene, disabling this one.");
            enabled = false; //Turn the script off and let somebody know
        }
        InvestigateChanged = new OnInvestigateChanged(LogInvestigateState);
        OnEnemySpawned = new OnEnemySpawn(LogEnemySpawn);
        if (BuildingsManager == null)
        {
            BuildingsManager = GameObject.FindObjectOfType<BuildingsManager>(); //Fetch the buildings script
        }
        BuildingsManager.FindBuildingsInGraph(); //Collect all of the graph's buildings together
        CreateSquads(); //Create all of the enemies
        //Now we need to give orders to all of our squads
        OrdersManager.StartingOrders(); //Assign initial orders
        foreach (EnemySquad squad in AllSquads)
        { //Now lets move all of the enemies to their assigned nodes
            squad.TeleportAll(squad.OrderContext.Node);
        }
    }

    

    void LogInvestigateState(bool investigating)
    {
        Debug.Log("Investigation has changed. New investigate value is " + investigating);
    }

    GameObject LogEnemySpawn(Enemy enemy)
    {
        Debug.Log("A new enemy has spawned.");
        return enemy.gameObject; //I don't entirely remember why I was doing it this way...
    }

    /// <summary>
    /// Create enemies for all the squads that need to be placed. Also handles assigning orders.
    /// </summary>
    public void CreateSquads()
    {
        foreach (SquadSO_Pair pair in SquadTemplates)
        {
            SquadScriptableObject SquadSO = pair.SquadSO;

            for (int x = 0; x < pair.Amount; x++)
            {
                EnemySquad squad = new EnemySquad(); //Set up the new squad object
                foreach (EnemyAmount enemy in SquadSO.EnemyTypes)
                {
                    int num = enemy.RollNumber(); //Determine the number of enemies in this squad type
                    for (int i = 0; i < num; i++)
                    { //Now spawn one of these enemies, assign it to its squad, and put it in the EnemyManager's list
                        GameObject newEnemy = GameObject.Instantiate(enemy.EnemyPrefab); //We won't do anything about its position until orders are assigned
                        EnemyManager.Instance.AllEnemies.Add(newEnemy.GetComponent<Enemy>());
                        squad.AddEnemy(newEnemy); //Track all instantiated enemies in the manager
                        squad.OrderWeights = SquadSO.PreferredOrders; //Copy the preferred order array over to the new squad
                        if (isServer)
                        {
                            Debug.Log("Spawning enemy across network");
                            NetworkServer.Spawn(newEnemy);
                        }
                        else
                        {
                            Debug.Log("EnemySpawner is not the host, can't spawn an enemy");
                        }
                    }
                }
                //Doing this in the for loop so the extra squads actually get added
                EnemyManager.Instance.AllSquads.Add(squad); //Put the new squad in the list
            }
        }
    }

}

[System.Serializable]
public class SquadSO_Pair
{
    public SquadScriptableObject SquadSO;
    public int Amount;
}