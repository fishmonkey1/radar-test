using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class EnemyManager : NetworkBehaviour
{

    static EnemyManager instance; //Lazy awake singleton pattern because lazy rn
    public static EnemyManager Instance => instance;

    public BuildingsManager BuildingsManager; //For grabbing map buildings and checking weights
    public EnemySpawner Spawner; //For fetching enemy squads out of
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
        BuildingsManager.FindBuildingsInGraph(); //Collect all of the graph's buildings together
        Spawner.CreateSquads(); //Create all of the enemies
        //Now we need to give orders to all of our squads
        OrdersManager.StartingOrders(); //Assign initial orders
        foreach (EnemySquad squad in AllSquads)
        { //Now lets move all of the enemies to their assigned nodes
            squad.TeleportAll(squad.OrderContext.Node);
        }
        //It makes more sense to spawn the enemies over the network here, now that they're positioned properly
        if (isServer)
        { //If we're on the server then we spawn all the enemies over the network
            foreach (Enemy enemy in AllEnemies)
            {
                NetworkServer.Spawn(enemy.gameObject); //Spawn baby, spawn!
            }
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

}
