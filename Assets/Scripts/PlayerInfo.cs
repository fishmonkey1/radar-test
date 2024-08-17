using System;
using UnityEngine;
using Mirror;

public class PlayerInfo : NetworkBehaviour
{
    /// <summary>
    /// Meant to be hold information that will be more relevant when the game becomes multiplayer
    /// </summary>
    [SyncVar] public string PlayerName;
    [SyncVar(hook = nameof(NetworkChangeRole))] public Role CurrentRole;
    public delegate void RoleChangeDelegate(Role oldRole, Role newRole);
    public RoleChangeDelegate OnRoleChange;
    public GameObject horniTank;

    public override void OnStartLocalPlayer()
    {
        Debug.Log("Starting up a local PlayerInfo component");
        if (isLocalPlayer && Utils.IsSceneActive(TankRoomManager.singleton.GameplayScene))
        {
            Debug.Log("This PlayerInfo is on a local player and we are in the gameplay screen");
            horniTank = GameObject.FindGameObjectWithTag("PlayerTank"); //Find the tank and assign it
            PickRole(CurrentRole);
        }

    }

    /// <summary>
    /// This function is called by the server when it changes the syncVar for CurrentRole
    /// </summary>
    /// <param name="oldRole"></param>
    /// <param name="newRole"></param>
    public void NetworkChangeRole(Role oldRole, Role newRole)
    {
        Debug.Log("SyncVar for Role changed. Picking new role.");
        Debug.Log($"oldRole is {oldRole.Name} and newRole is {newRole.Name}");
        PickRole(newRole);
    }

    public void PickRole(Role role)
    {
        //This will later need checks to make sure the picked role isn't over the limit
        Role oldRole = CurrentRole;
        CurrentRole = role;
        if (Utils.IsSceneActive(TankRoomManager.singleton.GameplayScene))
        {
            bool isLocal = GameObject.ReferenceEquals(gameObject, NetworkClient.localPlayer.gameObject);
            if (isLocal)
            {
                CamCycle.Instance.ChangeRoles(oldRole, role);
                Debug.Log("Setting up local player camera");
                if (horniTank != null)
                {
                    Debug.Log("Assigning player to spawned tank");
                    Debug.Log("Role equals gunner: " + (role.Name == CrewRoles.Gunner.Name));
                    if (role.Name == CrewRoles.Gunner.Name)
                        horniTank.GetComponent<Turret>().SetPlayer(this);
                    if (role.Name == CrewRoles.Driver.Name)
                        horniTank.GetComponent<tankSteer>().SetPlayer(this);
                }
            }
        }
        
        if (oldRole == null)
            Debug.Log($"Assigned role named {role.Name} to player");
        else
            Debug.Log($"Changed role from {oldRole.Name} to {role.Name}");
        if (OnRoleChange != null)
        {
            OnRoleChange(oldRole, role);
        }
    }

    public void PickName(string name)
    {
        PlayerName = name;
    }

    public void OnDebugChangeRoles()
    {
        int currentRoleIndex = Array.IndexOf(CrewRoles.ImplementedRoles, CurrentRole);
        currentRoleIndex++; //Increment to get the next role
        if (currentRoleIndex >= CrewRoles.ImplementedRoles.Length)
            currentRoleIndex = 0;

        PickRole(CrewRoles.ImplementedRoles[currentRoleIndex]); //Cycle the role over to the newly picked one
    }
}
