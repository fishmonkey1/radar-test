using Mirror;
using UnityEngine;

/// <summary>
/// Literally just holds a PlayerProfile reference and handles checking if the player is local
/// </summary>
public class ProfileHolder : MonoBehaviour
{

    public PlayerProfile Profile;

    void Awake()
    {
        Profile = new(); //Create a profile to hold
        Profile.Holder = this;
    }

    /// <summary>
    /// Check if this profile is the local client's gameobject
    /// </summary>
    /// <returns></returns>
    public bool IsLocalPlayer()
    {
        GameObject localObject = NetworkClient.localPlayer.gameObject;
        if (GameObject.ReferenceEquals(gameObject, localObject))
            return true;
        else
            return false;
    }

    private void OnDestroy()
    {
        if (Profile != null)
        {
            //We should unsubscribe the profile from the delegates it subscribed too
            Profile.Unsubscribe();
        }
    }
}
