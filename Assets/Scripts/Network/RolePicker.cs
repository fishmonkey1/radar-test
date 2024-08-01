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
    public readonly SyncDictionary<Role, uint> pickedRoles = new();

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
    { //Called on the server to pick a role and add to the SyncDict
        Debug.Log("Server ran the select role and updated the sync dict");
        Role pickedRole = CrewRoles.GetRoleByName(roleName);
        Role oldRole = OwnerHasRole(ownerID);
        if (oldRole != null)
        {
            pickedRoles.Remove(oldRole);
            Debug.Log("Removed old role with name " + oldRole.Name);
        }

        pickedRoles.Add(pickedRole, ownerID);
    }

    Role OwnerHasRole(uint ownerID)
    {
        foreach (KeyValuePair<Role, uint> pair in pickedRoles)
        {
            if (pair.Value == ownerID)
                return pair.Key;
        }
        return null;
    }

    public override void OnStartClient()
    {
        Debug.Log("Subscribed to sync dict callback");
        pickedRoles.Callback += OnPickedRoleChanged;
    }

    void OnPickedRoleChanged(SyncDictionary<Role, uint>.Operation op, Role role, uint ownerID)
    { //Called on observing clients when the dictionary has changed
        Debug.Log($"SyncDict changed. Values are operation: {op.ToString()}, Role: {role.Name}, owner: {ownerID}");
        /*switch (op) 
        {
            case SyncIDictionary<Role, uint>.Operation.OP_ADD:
                ChangeRoleButton(role, false); //Disable the picked role
                break;
            case SyncIDictionary<Role, uint>.Operation.OP_REMOVE:
                ChangeRoleButton(role, true); //Enable the picked role since its removed
                break;
        }*/
        UpdateButtons();
    }

    void UpdateButtons()
    {
        string syncDictKVPs = "";
        foreach (KeyValuePair<Role, uint> pair in pickedRoles)
        {
            syncDictKVPs += $"Key: {pair.Key.Name} Value: {pair.Value} ";
        }
        Debug.Log($"Updating buttons. IsClient: {isClient}, IsServer: {isServer}, SyncDict: {syncDictKVPs}");
        foreach (GameObject button in buttons)
        {
            TextMeshProUGUI text = button.GetComponentInChildren<TextMeshProUGUI>();
            Role role = CrewRoles.GetRoleByName(text.text);
            bool usedRole = false;
            foreach (KeyValuePair<Role, uint> pair in pickedRoles)
            {
                if (pair.Key.Name ==  role.Name)
                    usedRole = true; break;
            }
            if (usedRole)
                button.GetComponent<Button>().interactable = false;
            else
                button.GetComponent<Button>().interactable = true;
        }
    }

    void ChangeRoleButton(Role role, bool enable)
    { //If disable is true then we turn the button off, and the opposite for false
        foreach(GameObject button in buttons)
        {
            TextMeshProUGUI text = button.GetComponentInChildren<TextMeshProUGUI>();
            if (text.text == role.Name)
            { //We found it
                Button buttonComp = button.GetComponent<Button>();
                buttonComp.interactable = enable;
                break;
            }
        }
    }

}
