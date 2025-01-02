using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HorniTank
{
    [CreateAssetMenu(fileName = "Role", menuName = "ScriptableObjects/Role")]
    public class RoleScriptableObject : ScriptableObject
    {
        public Role Role;
        public string RoleName;
        public uint RoleId;
        public uint PlayerLimit;

        private void Awake()
        {
            Role = new Role(RoleName, RoleId, PlayerLimit);
            CrewRoles.TryAddRole(Role);
            Debug.Log($"Created new Role Scriptable Object in Awake(). Values of the created role are ID: {Role.ID}, Name: {Role.Name}, PlayerLimit: {Role.PlayerLimit}");
        }
    }
}
