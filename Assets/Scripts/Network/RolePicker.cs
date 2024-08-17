using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.SceneManagement;

public class RolePicker : NetworkBehaviour
{
    [SerializeField]
    RectTransform RoleButtonPanel; // Set the button panel in the inspector
    [SerializeField]
    GameObject RoleButtonPrefab;
    [SerializeField]
    Button ReadyButtonObject; //For enabling/disabling when you pick a role

    public readonly Dictionary<Role, uint> selectedRoles = new();

    List<GameObject> buttons = new List<GameObject>();
    TankRoomPlayer localRoomPlayer;

    void Start()
    {
        foreach (var role in CrewRoles.ImplementedRoles)
        { // Make a button for each implemented role
            GameObject newButton = GameObject.Instantiate(RoleButtonPrefab, RoleButtonPanel);
            TextMeshProUGUI buttonText = newButton.GetComponentInChildren<TextMeshProUGUI>();
            buttonText.text = role.Name;
            Button buttonScript = newButton.GetComponent<Button>();
            buttonScript.onClick.AddListener(() => SelectRole(role.Name));
            buttons.Add(newButton);
        }
        ReadyButtonObject.interactable = false; //Can't ready up until you pick a role
        //When the client enters the room we should update to reflect any roles that were picked before they joined
        CmdGetServerSelectedRoles(); //Ask for any roles that were picked so far
    }

    public void BackToMenuButton()
    {
        //Note to future me, we might want to change this to call the menu by name instead of build index
        if (isServer)
        { //You're hosting the game, so lets shut it down
            TankRoomManager.singleton.StopHost();
        }
        else if (isClient && !isServer)
        { //You've joined somebody else's game and need to leave
            TankRoomManager.singleton.StopClient();
        }
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
        //Fetch the local owner
        uint ownerID = NetworkClient.localPlayer.netId;
        Debug.Log($"Called select role with role name of {roleName} and an owner with netId of {ownerID}");
        if (localRoomPlayer == null)
            localRoomPlayer = NetworkClient.connection.identity.GetComponent<TankRoomPlayer>();
        //Try picking the role right away for the button update
        if (localRoomPlayer.role != null)
            localRoomPlayer.PickRole(localRoomPlayer.role.ID, CrewRoles.GetRoleByName(roleName).ID);
        else
            localRoomPlayer.PickRole(999, CrewRoles.GetRoleByName(roleName).ID);
        localRoomPlayer.CmdPickRole(CrewRoles.GetRoleByName(roleName).ID);
        CmdSelectRole(roleName, ownerID); //Send this to the server
    }

    [Command(requiresAuthority = false)]
    void CmdSelectRole(string roleName, uint ownerID)
    { //Called on the server to pick a role and add to the Dict
        Debug.Log("Server ran the select role and updated the sync dict");
        Role pickedRole = CrewRoles.GetRoleByName(roleName);
        Role oldRole = OwnerHasRole(ownerID);
        if (oldRole != null)
        {
            selectedRoles.Remove(oldRole);
            RpcRemoveRole(oldRole.Name);
            Debug.Log("Removed old role with name " + oldRole.Name);
        }

        selectedRoles.Add(pickedRole, ownerID);
        RpcSelectRole(roleName, ownerID);
        UpdateButtons();
    }

    [Command(requiresAuthority = false)]
    public void CmdRemoveRoleByID(uint id)
    {
        Role role = null;
        foreach (KeyValuePair<Role, uint> pair in selectedRoles)
        {
            if (pair.Value == id)
            {
                role = pair.Key;
                break;
            }
        }
        if (role == null)
            return; //This player didn't have a role selected
        if (selectedRoles.ContainsKey(role))
        {
            selectedRoles.Remove(role);
            RpcRemoveRole(role.Name);
        }
    }

    [ClientRpc]
    void RpcRemoveRole(string roleName)
    {
        Debug.Log("Client is running remove command with role " + roleName);
        if (isServer)
        {
            Debug.Log("Skipping remove call on host");
            return;
        }

        Role oldRole = CrewRoles.GetRoleByName(roleName);
        selectedRoles.Remove(oldRole);
        UpdateButtons(); //Refresh the buttons after the change
    }

    [ClientRpc]
    void RpcSelectRole(string roleName, uint ownerID)
    {
        if (isServer)
        {
            Debug.Log("Skipping add call for host.");
            return;
        }

        Role selectedRole = CrewRoles.GetRoleByName(roleName);
        selectedRoles.Add(selectedRole, ownerID);
        UpdateButtons(); //Refresh the buttons after the change
    }

    [Command(requiresAuthority = false)]
    public void CmdGetServerSelectedRoles()
    { //This collects the entries from the dictionary and then send them to the client
        if (selectedRoles.Count == 0)
            return; //No need to do this if there's no selected roles
        Role[] roles = new Role[selectedRoles.Count];
        uint[] ids = new uint[selectedRoles.Count];
        int index = 0;
        foreach (KeyValuePair<Role, uint> pair in selectedRoles)
        { //Build out the two arrays for sending
            roles[index] = pair.Key;
            ids[index] = pair.Value;
            index++;
        }
        RpcUpdateSelectedRoles(roles, ids);
    }

    [ClientRpc]
    public void RpcUpdateSelectedRoles(Role[] roles, uint[] ids)
    { //Called from the server after someone joins an in-progress room
        //TODO: Target this function to only players that have joined
        bool changed = false; //If there is an updated element then we update the buttons
        for (int i = 0; i < roles.Length; i++)
        { //Roles and ids array are the same size, so lets check for them in the local dictionary
            if (!selectedRoles.ContainsKey(roles[i]))
            {
                selectedRoles.Add(roles[i], ids[i]);
                changed = true;
            }
        }
        if (changed)
        { //This gets sent to every client right now, so only update buttons if there is new info
            UpdateButtons();
        }
    }

    Role OwnerHasRole(uint ownerID)
    {
        foreach (KeyValuePair<Role, uint> pair in selectedRoles)
        {
            if (pair.Value == ownerID)
                return pair.Key;
        }
        return null;
    }

    void UpdateButtons()
    {
        string syncDictKVPs = "";
        foreach (KeyValuePair<Role, uint> pair in selectedRoles)
        {
            syncDictKVPs += $"Key: {pair.Key.Name} Value: {pair.Value} ";
        }
        Debug.Log($"Updating buttons. IsClient: {isClient}, IsServer: {isServer}, SyncDict: {syncDictKVPs}");
        foreach (GameObject button in buttons)
        {
            TextMeshProUGUI text = button.GetComponentInChildren<TextMeshProUGUI>();
            Role role = CrewRoles.GetRoleByName(text.text);
            bool usedRole = false;
            foreach (KeyValuePair<Role, uint> pair in selectedRoles)
            {
                if (pair.Key.Name == role.Name)
                {
                    usedRole = true;
                    break;
                }
            }
            if (usedRole)
            {
                button.GetComponent<Button>().interactable = false;
                Debug.Log("Found a used role with name " + role.Name);
            }
            else
            {
                Debug.Log($"Role {role.Name} is unused.");
                button.GetComponent<Button>().interactable = true;
            }
        }
        if (localRoomPlayer == null) //Grab the room player before checking roles
            localRoomPlayer = NetworkClient.connection.identity.GetComponent<TankRoomPlayer>();
        if (localRoomPlayer.HasAnyRole())
        {
            Debug.Log("Local player has a role, turning ready button on.");
            ReadyButtonObject.interactable = true; //You picked a role so you can now ready up
        }
        else
        {
            Debug.Log($"No role on local player. RoleID is {localRoomPlayer.RoleID} and Role name is {localRoomPlayer.role.Name}");
        }
    }

}
