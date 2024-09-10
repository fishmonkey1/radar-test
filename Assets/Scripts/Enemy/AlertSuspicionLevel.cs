using UnityEngine;

public class AlertSuspicionLevel
{
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
    public delegate void SuspicionLevelDelegate(SuspicionLevel suspicion);
    public SuspicionLevelDelegate OnSuspicionLevelChanged;

    public AlertSuspicionLevel()
    {
        OnAlertLevelChanged = new AlertLevelDelegate(LogAlertChanged);
        OnSuspicionLevelChanged = new SuspicionLevelDelegate(LogSupsicionLevel);
    }

    void LogAlertChanged(AlertLevel level)
    {
        Debug.Log("Alert level changed. New alert level is: " + level.ToString());
    }

    void LogSupsicionLevel(SuspicionLevel level)
    {
        Debug.Log("Suspicion level changed. New suspicion level is " + level.ToString());
    }

}
