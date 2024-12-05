using UnityEngine;

/// <summary>
/// Outlines how the enemy AI should react to player spottings. Also contains enum definitions for different states.
/// </summary>
public class AlertSuspicionLevel
{
    /// <summary>
    /// How likely the enemy will be to respond aggressively to player spottings.
    /// </summary>
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
    /// <summary>
    /// When the alert level changes, this gets called. Useful for playing sounds later on.
    /// </summary>
    public AlertLevelDelegate OnAlertLevelChanged;

    /// <summary>
    /// How much the enemy will increase patrols and look for the players.
    /// </summary>
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
    public delegate void SuspicionLevelDelegate(SuspicionLevel suspicion);
    public SuspicionLevelDelegate OnSuspicionLevelChanged;

    public AlertSuspicionLevel()
    {
        OnAlertLevelChanged = new AlertLevelDelegate(LogAlertChanged);
        OnSuspicionLevelChanged = new SuspicionLevelDelegate(LogSuspicionLevel);
    }

    void LogAlertChanged(AlertLevel level)
    {
        Debug.Log("Alert level changed. New alert level is: " + level.ToString());
    }

    void LogSuspicionLevel(SuspicionLevel level)
    {
        Debug.Log("Suspicion level changed. New suspicion level is " + level.ToString());
    }

}
