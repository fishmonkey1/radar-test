using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Mirror;
using TMPro;

public class RolePicker : NetworkBehaviour
{
    [SerializeField]
    RectTransform RoleButtonPanel; // Set the button panel in the inspector
    [SerializeField]
    GameObject RoleButtonPrefab;
    public readonly Dictionary<Role, uint> selectedRoles = new();

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
            buttons.Add(newButton);
        }
    }

    void SelectRole(string roleName)
    {
        //Fetch the local owner
        uint ownerID = NetworkClient.localPlayer.netId;
        Debug.Log($"Called select role with role name of {roleName} and an owner with netId of {ownerID}");
        TankRoomPlayer localRoomPlayer = NetworkClient.connection.identity.GetComponent<TankRoomPlayer>();
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
    }

}
