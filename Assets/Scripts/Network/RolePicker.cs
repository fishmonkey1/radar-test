using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Network UI screen that lets players pick which role they want to have.
/// TODO: This screen will need to support multiple tanks in the future.
/// </summary>
public class RolePicker : NetworkBehaviour
{
    [SerializeField]
    RectTransform RoleButtonPanel; // Set the button panel in the inspector
    [SerializeField]
    GameObject RoleButtonPrefab;
    [SerializeField]
    Button ReadyButtonObject; //For enabling/disabling when you pick a role

    List<Role> selectedRoles = new();

    List<GameObject> buttons = new List<GameObject>();

    void Start()
    {
        foreach (var role in CrewRoles.ImplementedRoles)
        { // Make a button for each implemented role
            GameObject newButton = GameObject.Instantiate(RoleButtonPrefab, RoleButtonPanel);
            TextMeshProUGUI buttonText = newButton.GetComponentInChildren<TextMeshProUGUI>();
            buttonText.text = role.Name;
            Button buttonScript = newButton.GetComponent<Button>();
            buttonScript.onClick.AddListener(() => SelectRole(role.Name));
            if (role == CrewRoles.UnassignedRole)
            { //If we're rendering the unassigned button, it should render as clicked here at Start.
                buttonScript.interactable = false; //Can't assign yourself as unassigned when you start that way
            }
            buttons.Add(newButton);
            Debug.Log($"Added button for Role named {role.Name} and ID of {role.ID}");
        }
        ReadyButtonObject.interactable = false; //Can't ready up until you pick a role
        //When the client enters the room we should update to reflect any roles that were picked before they joined
        CmdGetServerSelectedRoles(); //Ask for any roles that were picked so far
    }

    public void BackToMenuButton()
    {
        if (isServer)
        { //You're hosting the game, so lets shut it down
            TankRoomManager.singleton.StopHost();
        }
        else if (isClient && !isServer)
        { //You've joined somebody else's game and need to leave
            TankRoomManager.singleton.StopClient();
        }
        //Note to future me, we might want to change this to call the menu by name instead of build index
        SceneManager.LoadScene(0); //Take us back to the main menu
    }

    public void ReadyButton()
    {
        TankRoomPlayer player = NetworkClient.localPlayer.GetComponent<TankRoomPlayer>();
        player.CmdChangeReadyState(true);
        if (NetworkClient.localPlayer == null)
            NetworkClient.AddPlayer();
    }

    void SelectRole(string roleName)
    {
        Debug.Log("Called select role locally, attempting to select role name " + roleName);
        //We need the server to confirm we got the role, so we just convert to a Role object and yeet it over to the server
        CmdSelectRole(CrewRoles.GetRoleByName(roleName), NetworkClient.localPlayer);
    }

    /// <summary>
    /// Called from the buttons in the rolepicker. Passes the desired role along and checks that it isn't picked on the server
    /// </summary>
    /// <param name="role">The role to be picked by the client</param>
    /// <param name="sender">The client that wants to take this role</param>
    [Command(requiresAuthority = false)]
    public void CmdSelectRole(Role role, NetworkIdentity sender)
    {
        //I'm gonna see if this works with using the sent Identity as a key. Maybe it will...
        PlayerProfile senderProfile = TankRoomManager.singleton.connectedPlayers[sender];
        Debug.Log($"Server is attempting to select a role named {role.Name} for player {senderProfile.PlayerName}");
        Role oldRole = senderProfile.CurrentRole;
        if (!selectedRoles.Contains(role))
        { //If it isn't in the list, then you can have it
            if (oldRole != CrewRoles.UnassignedRole)
            { //Take your old role out of the list as long as you had picked something
                selectedRoles.Remove(oldRole);
            }
            selectedRoles.Add(role); //And put your new role in the selected list
            senderProfile.SelectRole(role); //Make sure the server has you set up with the same role
            Debug.Log("Server assigned role to player.");
            if (!sender.isServer)
            {
                //Actually give the client the role, provided that they are not the host so we prevent duplicate messages
                TargetAssignRole(sender.connectionToClient, role);
            }
            RpcBroadcastSelectedRoles(selectedRoles); //And then update all clients about the changed list
        }
    }

    /// <summary>
    /// If a client succeeds in requesting a role, then this function gets called from CmdSelectRole.
    /// </summary>
    /// <param name="target">The client to receive the role</param>
    /// <param name="role">The role to give the client</param>
    [TargetRpc]
    void TargetAssignRole(NetworkConnection target, Role role)
    { //The server has confirmed that you got your role assigned
        PlayerProfile profile = NetworkClient.localPlayer.GetComponent<ProfileHolder>().Profile;
        profile.SelectRole(role);
        Debug.Log("Client received a role from the server and assigned it.");
    }

    /// <summary>
    /// Whenever the roles list is updated, this function gets called by the server to update the clients' lists.
    /// </summary>
    /// <param name="roles">The revised list from the server</param>
    [ClientRpc]
    void RpcBroadcastSelectedRoles(List<Role> roles)
    {
        if (!isServer)
        { //The server's list already has all the right stuff in it, so ignore the server for this
            selectedRoles = roles;
        }
        UpdateButtons();
    }

    [Command(requiresAuthority = false)]
    void CmdGetServerSelectedRoles(NetworkConnectionToClient conn = null)
    { //A client wants to know what roles have been set up, so lets send them the list
        TargetReceiveSelectedRoles(conn, selectedRoles);
    }

    /// <summary>
    /// Called from the server to deliver the updated selectedRoles list to a single requesting client
    /// </summary>
    /// <param name="target">The client who receives the updated list</param>
    /// <param name="roles">The list of roles the client will update to</param>
    [TargetRpc]
    void TargetReceiveSelectedRoles(NetworkConnection target,  List<Role> roles)
    {
        selectedRoles = roles;
        UpdateButtons();
    }

    void UpdateButtons()
    {
        Role localRole = NetworkClient.localPlayer.GetComponent<ProfileHolder>().Profile.CurrentRole;
        foreach (GameObject buttonObj in buttons)
        {
            //Get the text so we can figure out which role it represents
            TextMeshProUGUI text = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            Role buttonRole = CrewRoles.GetRoleByName(text.text);
            Button button = buttonObj.GetComponent<Button>();
            if (selectedRoles.Contains(buttonRole))
            { //This role has been picked, so turn off interactions
                button.interactable = false;
            }
            else
            { //Since the selectedRoles will NEVER contain the UnassignedRole, it will turn on provided you don't have it
                button.interactable = true;
            }
            if (buttonRole == CrewRoles.UnassignedRole && localRole == CrewRoles.UnassignedRole)
            { //If this is the Unassigned button and you have the role, we mark this button as unclickable, and the else statement handles turning it back on
                button.interactable = false;
            }
        }
        if (localRole != CrewRoles.UnassignedRole)
        { //You've picked a role, so you can ready up now
            ReadyButtonObject.interactable = true;
        }
    }

}
