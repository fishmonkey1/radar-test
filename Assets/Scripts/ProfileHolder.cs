using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Literally just holds a PlayerProfile reference and does nothing else
/// </summary>
public class ProfileHolder : MonoBehaviour
{

    public PlayerProfile Profile = new();

    void Awake()
    {
        if (Profile != null)
            Profile.Holder = this;
    }

    public bool IsLocalPlayer()
    {
        GameObject localObject = NetworkClient.localPlayer.gameObject;
        if (GameObject.ReferenceEquals(gameObject, localObject))
            return true;
        else
            return false;
    }
}
