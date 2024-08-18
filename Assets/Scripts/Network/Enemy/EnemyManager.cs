using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
    public AlertLevel Alert {  get; private set; }

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

    /// <summary>
    /// If a player has been spotted, this is true. Could potentially raise the alert level if spotted again
    /// </summary>
    public bool Investigating { get; private set; }
    /// <summary>
    /// How long the player should avoid enemies for the investigation to end only in suspicion
    /// </summary>
    public float InvestigateTimeLeft { get; private set; } 

    public struct Vector3FloatPair
    {
        public float number;
        public Vector3 vector;
    }
    //Stores where the player was seen and how much timethe search has left
    public List<Vector3FloatPair> Spottings {  get; private set; }

    public List<Enemy> Enemies = new();
    public delegate GameObject OnEnemySpawn(Enemy enemy);
    public OnEnemySpawn OnEnemySpawned;

    // Start is called before the first frame update
    void Awake()
    {
        if (singleton == null)
            singleton = this;
        else
        {
            Debug.Log("You have two EnemyManagers in the scene, disabling this one.");
            enabled = false; //Turn the script off and let somebody know
        }
    }



}
