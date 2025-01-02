using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Statically defined roles, including arrays for checking which roles are implemented.
/// </summary>
public static class CrewRoles
{
    public static readonly Role UnassignedRole = new Role("Unassigned", 0, 999);
    public static readonly Role Driver = new Role("Driver", 1, 1);
    public static readonly Role Gunner = new Role("Gunner", 2, 1);
    public static readonly Role Spotter = new Role("Spotter", 3, 1);
    public static readonly Role Radar = new Role("Radar", 4, 1);

    //Implemented roles list contains all the roles that actually work so far
    public static List<Role> ImplementedRoles { get; private set; } = new List<Role> { UnassignedRole, Driver, Gunner, Radar };
    //The AllRoles array might not be all that important, but I'm leaving it in for now
    public static List<Role> AllRoles { get; private set; } = new List<Role> { UnassignedRole, Driver, Gunner, Spotter, Radar };

    /// <summary>
    /// Find the static role instance based on an ID.
    /// </summary>
    /// <param name="id">The role ID you want to convert</param>
    /// <returns>The statically defined role the ID matches</returns>
    /// <exception cref="System.Exception">Thrown if there isn't a defined Role for the ID passed.</exception>
    public static Role GetRoleByID(uint id)
    {
        foreach (Role role in AllRoles)
        {
            if (role.ID == id)
                return role;
        }
        //Yeet an error at them if no role is found
        throw new System.Exception($"No Role ID exists for passed ID {id} in CrewRoles!");
    }

    /// <summary>
    /// Find a role based on their name, used mostly by buttons.
    /// </summary>
    /// <param name="name">Name of the role</param>
    /// <returns>The statically defined role with a matching name</returns>
    /// <exception cref="System.Exception">Thrown if no role is found for passed name.</exception>
    public static Role GetRoleByName(string name)
    {
        foreach (Role role in AllRoles)
        {
            if (role.Name == name)
                return role;
        }
        throw new System.Exception($"No Role named {name} exists in CrewRoles!");
    }

    public static bool TryAddRole(Role role)
    {
        bool added = false;
        if (!ImplementedRoles.Contains(role))
        {
            ImplementedRoles.Add(role);
            added = true;
        }
        if (!AllRoles.Contains(role))
        {
            AllRoles.Add(role);
            added = true;
        }
        if (added)
            Debug.Log($"Added a new role to the CrewRoles internal lists. New Role information is Name: {role.Name}, ID: {role.ID}, PlayerLimit: {role.PlayerLimit}");
        return added;
    }

}

/// <summary>
/// Represents a role that the player can take, including what the displayed name is, the role's internal ID, and the number of players that can take the role per vehicle.
/// </summary>
[System.Serializable]
public class Role
{
    public readonly string Name;
    public readonly uint ID;
    public readonly uint PlayerLimit;

    public Role() { }

    public Role(string name, uint id, uint limit)
    {
        Name = name;
        ID = id;
        PlayerLimit = limit;
    }

    public override bool Equals(object obj)
    {
        if (obj is Role otherRole)
        {
            return ID == otherRole.ID;
        }
        return false;
    }

    //Override GetHashCode to align with overriden Equals statement
    public override int GetHashCode()
    {
        return ID.GetHashCode();
    }

    //Overload equality operator for niceness in comparing roles
    public static bool operator ==(Role leftRole, Role rightRole)
    {
        if (ReferenceEquals(leftRole, rightRole)) return true;
        if (ReferenceEquals(leftRole, null) || ReferenceEquals(rightRole, null)) return false;
        return leftRole.ID == rightRole.ID;
    }

    //And overload inequality operator so we don't get yelled at in the above overload
    public static bool operator !=(Role leftRole, Role rightRole)
    { //Just do the equality operator but flipped
        return !(leftRole == rightRole);
    }
}
