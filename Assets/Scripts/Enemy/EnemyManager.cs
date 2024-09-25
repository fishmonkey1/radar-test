using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{

    static EnemyManager singleton; //Lazy awake singleton pattern because lazy rn
    public static EnemyManager Singleton => singleton;

    public BuildingsManager BuildingsManager; //For grabbing map buildings and checking weights
    public EnemySpawner Spawner; //For fetching enemy squads out of

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
        if (singleton == null)
            singleton = this;
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
        StartingOrders(); //Assign initial orders
    }

    public void StartingOrders()
    { //We assign all of the orders we need in here and place the enemies at their assigned node
        foreach (EnemySquad squad in AllSquads)
        {

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
