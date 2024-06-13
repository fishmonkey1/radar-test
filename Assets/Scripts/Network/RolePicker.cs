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
    public readonly SyncDictionary<Role, NetworkIdentity> pickedRoles = new();

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
        NetworkIdentity owner = NetworkClient.connection.identity;
        CmdSelectRole(roleName, owner); //Send this to the server
    }

    [Command(requiresAuthority = false)]
    void CmdSelectRole(string roleName, NetworkIdentity owner)
    { //Called on the server to pick a role and add to the SyncDict
        Role pickedRole = CrewRoles.GetRoleByName(roleName);
        pickedRoles.Add(pickedRole, owner);
    }

    public override void OnStartClient()
    {
        pickedRoles.Callback += OnPickedRoleChanged;
    }

    void OnPickedRoleChanged(SyncDictionary<Role, NetworkIdentity>.Operation op, Role role, NetworkIdentity owner)
    { //Called on observing clients when the dictionary has changed
        switch (op) 
        {
            case SyncIDictionary<Role, NetworkIdentity>.Operation.OP_ADD:
                ChangeRoleButton(role, false); //Disable the picked role
                break;
            case SyncIDictionary<Role, NetworkIdentity>.Operation.OP_REMOVE:
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
                button.SetActive(enable);
                break;
            }
        }
    }

}
