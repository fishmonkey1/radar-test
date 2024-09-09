using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{

    static EnemyManager singleton; //Lazy singleton pattern because lazy rn
    public static EnemyManager Singleton => singleton;

    public enum AlertLevel
    {
        NONE,
        LOW,
        MEDIUM,
        HIGH,
        LOCKDOWN
    }
    public AlertLevel Alert { get; private set; }
    public delegate void AlertLevelDelegate(AlertLevel alertLevel);
    public AlertLevelDelegate OnAlertLevelChanged;

    public enum SuspicionLevel
    { //If the player gets spotted frequently but doesn't get caught the SuspicionLevel goes up
        ///potentially raises AlertLevel
        NONE,
        SLIGHT,
        SEARCH,
        REINFORCE,
        ALERT, //This raises the AlertLevel and drops suspicion to search
    }
    public SuspicionLevel Suspicion { get; private set; }
    public delegate void OnSuspicionLevelChanged(SuspicionLevel suspicion);
    public OnSuspicionLevelChanged SuspicionLevelChanged;


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

    void Awake()
    {
        if (singleton == null)
            singleton = this;
        else
        {
            Debug.Log("You have two EnemyManagers in the scene, disabling this one.");
            enabled = false; //Turn the script off and let somebody know
        }
        OnAlertLevelChanged = new AlertLevelDelegate(LogAlertChanged);
        SuspicionLevelChanged = new OnSuspicionLevelChanged(LogSupsicionLevel);
        InvestigateChanged = new OnInvestigateChanged(LogInvestigateState);
        OnEnemySpawned = new OnEnemySpawn(LogEnemySpawn);
    }

    void LogAlertChanged(AlertLevel level)
    {
        Debug.Log("Alert level changed. New alert level is: " + level.ToString());
    }

    void LogSupsicionLevel(SuspicionLevel level)
    {
        Debug.Log("Suspicion level changed. New suspicion level is " + level.ToString());
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
