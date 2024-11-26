using Mirror;
using System.Collections;
using System.Collections.Generic;
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

    public bool IsLocalPlayer()
    {
        GameObject localObject = NetworkClient.localPlayer.gameObject;
        if (GameObject.ReferenceEquals(gameObject, localObject))
            return true;
        else
            return false;
    }
}
