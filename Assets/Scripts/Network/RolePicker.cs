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
    public readonly SyncDictionary<Role, int> pickedRoles = new();

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
        if (isClient && isServer)
        {
            Debug.Log("Detected host and subscribed to callback");
            pickedRoles.Callback += OnPickedRoleChanged;
        }
    }

    void SelectRole(string roleName)
    {
        //Fetch the local owner
        int ownerID = NetworkClient.connection.connectionId;
        Debug.Log($"Called select role with role name of {roleName} and an owner with value {ownerID}");
        CmdSelectRole(roleName, ownerID); //Send this to the server
    }

    [Command(requiresAuthority = false)]
    void CmdSelectRole(string roleName, int ownerID)
    { //Called on the server to pick a role and add to the SyncDict
        Debug.Log("Server ran the select role and updated the sync dict");
        Role pickedRole = CrewRoles.GetRoleByName(roleName);
        pickedRoles.Add(pickedRole, ownerID);
    }

    public override void OnStartClient()
    {
        Debug.Log("Subscribed to sync dict callback");
        pickedRoles.Callback += OnPickedRoleChanged;
    }

    void OnPickedRoleChanged(SyncDictionary<Role, int>.Operation op, Role role, int ownerID)
    { //Called on observing clients when the dictionary has changed
        Debug.Log($"SyncDict changed. Values are operation: {op.ToString()}, Role: {role.Name}, owner: {ownerID}");
        switch (op) 
        {
            case SyncIDictionary<Role, int>.Operation.OP_ADD:
                ChangeRoleButton(role, false); //Disable the picked role
                break;
            case SyncIDictionary<Role, int>.Operation.OP_REMOVE:
                ChangeRoleButton(role, true); //Enable the picked role since its removed
                break;
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
